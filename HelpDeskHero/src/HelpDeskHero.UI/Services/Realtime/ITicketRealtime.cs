using HelpDeskHero.Shared.Contracts.Tickets;

namespace HelpDeskHero.UI.Services.Realtime;

public interface ITicketRealtime
{
    Task<IAsyncDisposable> SubscribeDashboardAsync(Func<TicketLiveUpdateDto, Task> handler, CancellationToken cancellationToken = default);

    Task<IAsyncDisposable> SubscribeTicketAsync(int ticketId, Func<TicketLiveUpdateDto, Task> handler, CancellationToken cancellationToken = default);
}
