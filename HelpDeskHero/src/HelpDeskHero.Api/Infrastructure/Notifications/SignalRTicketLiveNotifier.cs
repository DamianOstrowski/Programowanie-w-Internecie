using HelpDeskHero.Api.Application.Interfaces;
using HelpDeskHero.Api.Hubs;
using HelpDeskHero.Shared.Contracts.Tickets;
using Microsoft.AspNetCore.SignalR;

namespace HelpDeskHero.Api.Infrastructure.Notifications;

public sealed class SignalRTicketLiveNotifier : ITicketLiveNotifier
{
    private readonly IHubContext<TicketsHub> _hubContext;

    public SignalRTicketLiveNotifier(IHubContext<TicketsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyTicketChangedAsync(TicketLiveUpdateDto dto, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group("dashboard").SendAsync("TicketChanged", dto, ct);
        await _hubContext.Clients.Group($"ticket:{dto.TicketId}").SendAsync("TicketChanged", dto, ct);
    }
}
