using HelpDeskHero.Api.BackgroundJobs.Contracts;
using HelpDeskHero.Api.Infrastructure.Notifications;
using HelpDeskHero.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.BackgroundJobs;

public sealed class NotificationJob : INotificationJob
{
    private readonly AppDbContext _db;
    private readonly INotificationDispatcher _dispatcher;

    public NotificationJob(AppDbContext db, INotificationDispatcher dispatcher)
    {
        _db = db;
        _dispatcher = dispatcher;
    }

    public async Task SendTicketCreatedNotificationsAsync(int ticketId, CancellationToken ct = default)
    {
        var ticket = await _db.Tickets
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == ticketId, ct);

        if (ticket is null)
            return;

        var subject = $"Nowe zgłoszenie: {ticket.Number}";
        var body = $"Utworzono ticket {ticket.Number} - {ticket.Title}";
        var targetUserId = string.IsNullOrWhiteSpace(ticket.AssignedToUserId)
            ? ticket.CreatedByUserId
            : ticket.AssignedToUserId;

        await _dispatcher.DispatchAsync(new NotificationMessage
        {
            Channel = NotificationChannel.InApp,
            Subject = subject,
            Body = body,
            UserId = targetUserId
        }, ct);
    }

    public async Task SendDailySummaryAsync(CancellationToken ct = default)
    {
        var openCount = await _db.Tickets.CountAsync(x => !x.IsDeleted && x.Status != "Closed", ct);

        await _dispatcher.DispatchAsync(new NotificationMessage
        {
            Channel = NotificationChannel.InApp,
            Subject = "HelpDeskHero - Daily Summary",
            Body = $"Open tickets: {openCount}",
            UserId = null
        }, ct);
    }
}
