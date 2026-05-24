using HelpDeskHero.Shared.Contracts.Tickets;

namespace HelpDeskHero.UI.Services.Realtime;

public sealed class NoOpTicketRealtime : ITicketRealtime
{
    public Task<IAsyncDisposable> SubscribeDashboardAsync(Func<TicketLiveUpdateDto, Task> handler, CancellationToken cancellationToken = default) =>
        Task.FromResult<IAsyncDisposable>(NoOpAsyncDisposable.Instance);

    public Task<IAsyncDisposable> SubscribeTicketAsync(int ticketId, Func<TicketLiveUpdateDto, Task> handler, CancellationToken cancellationToken = default) =>
        Task.FromResult<IAsyncDisposable>(NoOpAsyncDisposable.Instance);

    private sealed class NoOpAsyncDisposable : IAsyncDisposable
    {
        internal static readonly NoOpAsyncDisposable Instance = new();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
