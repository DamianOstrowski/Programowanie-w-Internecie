using System.Text.Json;
using HelpDeskHero.Api.Application.Interfaces;
using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;

namespace HelpDeskHero.Api.Application.Services;

public sealed class OutboxWriter : IOutboxWriter
{
    private readonly AppDbContext _db;

    public OutboxWriter(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(string type, object payload, CancellationToken ct = default)
    {
        var msg = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            Type = type,
            Payload = JsonSerializer.Serialize(payload),
            RetryCount = 0
        };

        _db.OutboxMessages.Add(msg);
        return Task.CompletedTask;
    }
}
