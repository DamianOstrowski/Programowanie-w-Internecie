using HelpDeskHero.Shared.Contracts.Tickets;
using HelpDeskHero.UI.Services.Auth;
using Microsoft.AspNetCore.SignalR.Client;

namespace HelpDeskHero.UI.Services.Realtime;

public sealed class TicketSignalRRealtime : ITicketRealtime, IAsyncDisposable
{
    private readonly TokenStore _tokens;
    private readonly string _hubUrl;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _sync = new();
    private readonly List<Func<TicketLiveUpdateDto, Task>> _handlers = new();
    private readonly Dictionary<int, int> _ticketJoinCounts = new();
    private HubConnection? _connection;
    private int _dashboardJoinCount;

    public TicketSignalRRealtime(TokenStore tokens, IConfiguration configuration)
    {
        _tokens = tokens;
        var baseUrl = configuration["Api:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Missing Api:BaseUrl.");
        _hubUrl = $"{baseUrl}/hubs/tickets";
    }

    public async Task<IAsyncDisposable> SubscribeDashboardAsync(
        Func<TicketLiveUpdateDto, Task> handler,
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
            _handlers.Add(handler);

        var hub = await EnsureStartedAsync(cancellationToken).ConfigureAwait(false);

        var firstDashboard = false;
        lock (_sync)
        {
            if (_dashboardJoinCount == 0)
                firstDashboard = true;
            _dashboardJoinCount++;
        }

        if (firstDashboard)
            await hub.InvokeAsync("JoinDashboard", cancellationToken).ConfigureAwait(false);

        return new Subscription(this, handler, null);
    }

    public async Task<IAsyncDisposable> SubscribeTicketAsync(
        int ticketId,
        Func<TicketLiveUpdateDto, Task> handler,
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
            _handlers.Add(handler);

        var hub = await EnsureStartedAsync(cancellationToken).ConfigureAwait(false);

        var firstForTicket = false;
        lock (_sync)
        {
            if (!_ticketJoinCounts.TryGetValue(ticketId, out var c))
                c = 0;
            if (c == 0)
                firstForTicket = true;
            _ticketJoinCounts[ticketId] = c + 1;
        }

        if (firstForTicket)
            await hub.InvokeAsync("JoinTicket", ticketId.ToString(), cancellationToken).ConfigureAwait(false);

        return new Subscription(this, handler, ticketId);
    }

    private Task OnTicketChangedAsync(TicketLiveUpdateDto dto)
    {
        List<Func<TicketLiveUpdateDto, Task>> copy;
        lock (_sync)
            copy = _handlers.ToList();

        return Task.WhenAll(copy.Select(h => SafeInvokeAsync(h, dto)));
    }

    private static async Task SafeInvokeAsync(Func<TicketLiveUpdateDto, Task> handler, TicketLiveUpdateDto dto)
    {
        try
        {
            await handler(dto).ConfigureAwait(false);
        }
        catch
        {
            // Realtime is best-effort; avoid tearing down the hub on UI handler errors.
        }
    }

    private async Task<HubConnection> EnsureStartedAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
            return _connection;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_connection is not null)
                return _connection;

            var conn = new HubConnectionBuilder()
                .WithUrl(_hubUrl, opts =>
                {
                    opts.AccessTokenProvider = async () =>
                        await _tokens.GetAccessTokenAsync().ConfigureAwait(false) ?? string.Empty;
                })
                .WithAutomaticReconnect()
                .Build();

            conn.On<TicketLiveUpdateDto>("TicketChanged", OnTicketChangedAsync);

            await conn.StartAsync(cancellationToken).ConfigureAwait(false);
            _connection = conn;
            return conn;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async ValueTask UnsubscribeAsync(Func<TicketLiveUpdateDto, Task> handler, int? ticketId)
    {
        HubConnection? hub;
        lock (_sync)
        {
            _handlers.Remove(handler);
            hub = _connection;
        }

        if (hub is null)
            return;

        if (ticketId is { } tid)
        {
            var leave = false;
            lock (_sync)
            {
                if (_ticketJoinCounts.TryGetValue(tid, out var c))
                {
                    c--;
                    if (c <= 0)
                    {
                        _ticketJoinCounts.Remove(tid);
                        leave = true;
                    }
                    else
                        _ticketJoinCounts[tid] = c;
                }
            }

            if (leave)
            {
                try
                {
                    await hub.InvokeAsync("LeaveTicket", tid.ToString()).ConfigureAwait(false);
                }
                catch
                {
                    // ignore
                }
            }
        }
        else
        {
            lock (_sync)
            {
                if (_dashboardJoinCount > 0)
                    _dashboardJoinCount--;
            }
        }

        var shouldStop = false;
        lock (_sync)
            shouldStop = _handlers.Count == 0;

        if (!shouldStop)
            return;

        try
        {
            await hub.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }

        lock (_sync)
        {
            _connection = null;
            _dashboardJoinCount = 0;
            _ticketJoinCounts.Clear();
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        HubConnection? hub;
        lock (_sync)
        {
            hub = _connection;
            _connection = null;
            _handlers.Clear();
            _dashboardJoinCount = 0;
            _ticketJoinCounts.Clear();
        }

        if (hub is not null)
            await hub.DisposeAsync().ConfigureAwait(false);

        _gate.Dispose();
    }

    private sealed class Subscription : IAsyncDisposable
    {
        private readonly TicketSignalRRealtime _owner;
        private readonly Func<TicketLiveUpdateDto, Task> _handler;
        private readonly int? _ticketId;
        private int _disposed;

        public Subscription(TicketSignalRRealtime owner, Func<TicketLiveUpdateDto, Task> handler, int? ticketId)
        {
            _owner = owner;
            _handler = handler;
            _ticketId = ticketId;
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            await _owner.UnsubscribeAsync(_handler, _ticketId).ConfigureAwait(false);
        }
    }
}
