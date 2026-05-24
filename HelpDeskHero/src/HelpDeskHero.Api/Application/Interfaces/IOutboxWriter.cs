namespace HelpDeskHero.Api.Application.Interfaces;

public interface IOutboxWriter
{
    Task AddAsync(string type, object payload, CancellationToken ct = default);
}
