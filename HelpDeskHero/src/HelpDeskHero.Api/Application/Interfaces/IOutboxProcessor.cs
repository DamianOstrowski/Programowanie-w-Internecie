namespace HelpDeskHero.Api.Application.Interfaces;

public interface IOutboxProcessor
{
    Task ProcessPendingAsync(CancellationToken ct = default);
}
