namespace HelpDeskHero.Shared.Contracts.Tickets;

public sealed class TicketAttachmentDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
}
