using System.Text.Json;
using HelpDeskHero.Api.Application.Interfaces;
using HelpDeskHero.Api.BackgroundJobs.Contracts;
using HelpDeskHero.Api.Infrastructure.Notifications;
using HelpDeskHero.Api.Infrastructure.Persistence;
using HelpDeskHero.Shared.Contracts.Tickets;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Application.Services;

public sealed class OutboxProcessor : IOutboxProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _db;
    private readonly ITicketLiveNotifier _notifier;
    private readonly INotificationJob _notificationJob;
    private readonly INotificationDispatcher _dispatcher;

    public OutboxProcessor(
        AppDbContext db,
        ITicketLiveNotifier notifier,
        INotificationJob notificationJob,
        INotificationDispatcher dispatcher)
    {
        _db = db;
        _notifier = notifier;
        _notificationJob = notificationJob;
        _dispatcher = dispatcher;
    }

    public async Task ProcessPendingAsync(CancellationToken ct = default)
    {
        var messages = await _db.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null && x.RetryCount < 10)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(50)
            .ToListAsync(ct);

        foreach (var msg in messages)
        {
            try
            {
                if (msg.Type == "TicketChanged")
                {
                    var dto = JsonSerializer.Deserialize<TicketLiveUpdateDto>(msg.Payload, JsonOptions);
                    if (dto is not null)
                    {
                        await _notifier.NotifyTicketChangedAsync(dto, ct);

                        if (string.Equals(dto.EventType, "Created", StringComparison.OrdinalIgnoreCase))
                            await _notificationJob.SendTicketCreatedNotificationsAsync(dto.TicketId, ct);

                        if (string.Equals(dto.EventType, "SlaBreached", StringComparison.OrdinalIgnoreCase))
                            await DispatchSlaAsync(dto, ct);
                    }
                }

                msg.ProcessedAtUtc = DateTime.UtcNow;
                msg.Error = null;
            }
            catch (Exception ex)
            {
                msg.RetryCount++;
                msg.Error = ex.Message;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task DispatchSlaAsync(TicketLiveUpdateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.AssignedToUserId))
            return;

        await _dispatcher.DispatchAsync(new NotificationMessage
        {
            Channel = NotificationChannel.InApp,
            Subject = $"SLA: ticket #{dto.TicketId}",
            Body = $"Naruszenie SLA — priorytet {dto.Priority}, poziom eskalacji {dto.EscalationLevel}.",
            UserId = dto.AssignedToUserId
        }, ct);
    }
}
