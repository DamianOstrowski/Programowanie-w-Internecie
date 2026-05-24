using HelpDeskHero.Api.Domain;
using HelpDeskHero.Shared.Contracts.Tickets;

namespace HelpDeskHero.Api.Application;

public static class TicketLiveUpdateFactory
{
    public static TicketLiveUpdateDto Create(Ticket ticket, string eventType) =>
        new()
        {
            TicketId = ticket.Id,
            EventType = eventType,
            Status = ticket.Status,
            Priority = ticket.Priority,
            AssignedToUserId = ticket.AssignedToUserId,
            EscalationLevel = ticket.EscalationLevel,
            ChangedAtUtc = DateTime.UtcNow
        };
}
