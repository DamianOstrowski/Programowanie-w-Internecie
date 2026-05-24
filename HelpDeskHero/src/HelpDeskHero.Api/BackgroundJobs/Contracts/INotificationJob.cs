namespace HelpDeskHero.Api.BackgroundJobs.Contracts;

public interface INotificationJob
{
    Task SendTicketCreatedNotificationsAsync(int ticketId, CancellationToken ct = default);
    Task SendDailySummaryAsync(CancellationToken ct = default);
}
