using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;

namespace HelpDeskHero.Api.Infrastructure.Notifications;

public sealed class InAppNotificationSender : INotificationSender
{
    private readonly AppDbContext _db;

    public InAppNotificationSender(AppDbContext db)
    {
        _db = db;
    }

    public NotificationChannel Channel => NotificationChannel.InApp;

    public async Task SendAsync(NotificationMessage message, CancellationToken ct = default)
    {
        var entity = new UserNotification
        {
            UserId = message.UserId,
            Subject = message.Subject,
            Body = message.Body,
            CreatedAtUtc = DateTime.UtcNow,
            IsRead = false
        };

        _db.UserNotifications.Add(entity);
        await _db.SaveChangesAsync(ct);
    }
}
