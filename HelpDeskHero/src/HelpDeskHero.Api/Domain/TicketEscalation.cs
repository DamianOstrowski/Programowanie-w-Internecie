namespace HelpDeskHero.Api.Domain;

public sealed class TicketEscalation
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = default!;

    public int EscalationLevel { get; set; }
    public DateTime TriggeredAtUtc { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AssignedToUserId { get; set; }
    public bool NotificationSent { get; set; }
}
