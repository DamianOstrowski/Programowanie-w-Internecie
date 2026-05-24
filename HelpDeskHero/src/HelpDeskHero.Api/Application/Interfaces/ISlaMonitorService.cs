namespace HelpDeskHero.Api.Application.Interfaces;

public interface ISlaMonitorService
{
    Task CheckBreachesAsync(CancellationToken ct = default);
}
