namespace HelpDeskHero.Shared.Contracts.Notifications;

public sealed class NotificationDto
{
    public int Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "Queued";
    public DateTime CreatedAtUtc { get; set; }
}
