namespace HelpDeskHero.Api.Infrastructure.Services;

public interface INotificationQueue
{
    void EnqueueTicketCreated(int ticketId);
}
