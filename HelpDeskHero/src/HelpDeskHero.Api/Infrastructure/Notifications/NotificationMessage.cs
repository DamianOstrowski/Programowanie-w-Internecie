namespace HelpDeskHero.Api.Infrastructure.Notifications;

public sealed class NotificationMessage
{
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public string? UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
