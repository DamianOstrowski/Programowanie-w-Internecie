namespace HelpDeskHero.Api.Infrastructure.Notifications;

public sealed class EmailNotificationSender : INotificationSender
{
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(ILogger<EmailNotificationSender> logger)
    {
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Email;

    public Task SendAsync(NotificationMessage message, CancellationToken ct = default)
    {
        _logger.LogInformation("Email notification stub: {Subject}", message.Subject);
        return Task.CompletedTask;
    }
}
