namespace HelpDeskHero.Api.Infrastructure.Notifications;

public interface INotificationSender
{
    NotificationChannel Channel { get; }
    Task SendAsync(NotificationMessage message, CancellationToken ct = default);
}
