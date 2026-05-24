namespace HelpDeskHero.Api.Infrastructure.Services;

public sealed class NoOpNotificationQueue : INotificationQueue
{
    public void EnqueueTicketCreated(int ticketId)
    {
    }
}
