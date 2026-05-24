using HelpDeskHero.Api.Application;
using HelpDeskHero.Api.Application.Interfaces;
using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Application.Services;

public sealed class SlaMonitorService : ISlaMonitorService
{
    private readonly AppDbContext _db;
    private readonly IOutboxWriter _outboxWriter;
    private static readonly TimeSpan EscalationThrottle = TimeSpan.FromMinutes(15);

    public SlaMonitorService(AppDbContext db, IOutboxWriter outboxWriter)
    {
        _db = db;
        _outboxWriter = outboxWriter;
    }

    public async Task CheckBreachesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var tickets = await _db.Tickets
            .Where(x => !x.IsDeleted && x.Status != "Closed")
            .ToListAsync(ct);

        foreach (var ticket in tickets)
        {
            var resolveBreached = ticket.DueResolveAtUtc.HasValue && ticket.DueResolveAtUtc < now;
            var firstBreached = ticket.DueFirstResponseAtUtc is { } dueFr
                && ticket.FirstRespondedAtUtc is null
                && dueFr < now;

            if (!resolveBreached && !firstBreached)
                continue;

            if (ticket.LastNotifiedAtUtc.HasValue && (now - ticket.LastNotifiedAtUtc.Value) < EscalationThrottle)
                continue;

            var reason = resolveBreached ? "Resolve SLA breached." : "First response SLA breached.";
            ticket.EscalationLevel++;
            ticket.LastNotifiedAtUtc = now;

            _db.TicketEscalations.Add(new TicketEscalation
            {
                TicketId = ticket.Id,
                EscalationLevel = ticket.EscalationLevel,
                TriggeredAtUtc = now,
                Reason = reason,
                AssignedToUserId = ticket.AssignedToUserId,
                NotificationSent = false
            });

            await _outboxWriter.AddAsync("TicketChanged", TicketLiveUpdateFactory.Create(ticket, "SlaBreached"), ct);
        }

        await _db.SaveChangesAsync(ct);
    }
}
