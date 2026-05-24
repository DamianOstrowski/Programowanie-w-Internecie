namespace HelpDeskHero.Shared.Contracts.Notifications;

public sealed class UserNotificationDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
