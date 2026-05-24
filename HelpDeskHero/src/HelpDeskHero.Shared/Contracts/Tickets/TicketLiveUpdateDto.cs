namespace HelpDeskHero.Shared.Contracts.Tickets;

public sealed class TicketLiveUpdateDto
{
    public int TicketId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? AssignedToUserId { get; set; }
    public int EscalationLevel { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}
