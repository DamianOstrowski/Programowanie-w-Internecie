namespace HelpDeskHero.Api.Infrastructure.Notifications;

public sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly IReadOnlyDictionary<NotificationChannel, INotificationSender> _senders;

    public NotificationDispatcher(IEnumerable<INotificationSender> senders)
    {
        _senders = senders.ToDictionary(x => x.Channel, x => x);
    }

    public async Task DispatchAsync(NotificationMessage message, CancellationToken ct = default)
    {
        if (!_senders.TryGetValue(message.Channel, out var sender))
            throw new InvalidOperationException($"Missing sender for channel {message.Channel}.");

        await sender.SendAsync(message, ct);
    }
}
