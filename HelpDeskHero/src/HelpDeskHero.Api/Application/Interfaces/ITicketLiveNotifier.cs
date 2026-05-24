using HelpDeskHero.Shared.Contracts.Tickets;

namespace HelpDeskHero.Api.Application.Interfaces;

public interface ITicketLiveNotifier
{
    Task NotifyTicketChangedAsync(TicketLiveUpdateDto dto, CancellationToken ct = default);
}
