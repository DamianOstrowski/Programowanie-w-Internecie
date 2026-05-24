using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HelpDeskHero.Api.Hubs;

[Authorize]
public sealed class TicketsHub : Hub
{
    public Task JoinDashboard() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");

    public Task JoinTicket(string ticketId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"ticket:{ticketId}");

    public Task LeaveTicket(string ticketId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket:{ticketId}");
}
