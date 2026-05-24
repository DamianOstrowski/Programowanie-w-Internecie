using HelpDeskHero.Api.Domain;

namespace HelpDeskHero.Api.Application.Interfaces;

public interface ISlaCalculator
{
    Task ApplySlaAsync(Ticket ticket, CancellationToken ct = default);
}
