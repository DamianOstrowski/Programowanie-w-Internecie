namespace HelpDeskHero.Api.Domain;

public sealed class UserNotification
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
