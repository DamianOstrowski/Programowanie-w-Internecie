namespace HelpDeskHero.Shared.Contracts.Tickets;

public sealed class TicketCommentDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedByDisplayName { get; set; } = string.Empty;
}

public sealed class CreateTicketCommentDto
{
    public string Body { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
