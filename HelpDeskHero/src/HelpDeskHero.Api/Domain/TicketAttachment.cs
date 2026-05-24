namespace HelpDeskHero.Api.Domain;

public sealed class TicketAttachment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = default!;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
}
