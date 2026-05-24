namespace HelpDeskHero.Shared.Contracts.Tickets;

public sealed class TicketSlaDto
{
    public int TicketId { get; set; }
    public DateTime? DueFirstResponseAtUtc { get; set; }
    public DateTime? DueResolveAtUtc { get; set; }
    public bool FirstResponseBreached { get; set; }
    public bool ResolveBreached { get; set; }
    public int EscalationLevel { get; set; }
}
