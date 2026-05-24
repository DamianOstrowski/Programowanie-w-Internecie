using System.Security.Claims;
using System.Text.Json;
using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;

namespace HelpDeskHero.Api.Infrastructure.Services;

public sealed class AuditService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task WriteAsync(string action, string entityName, string entityId, object? details = null, CancellationToken ct = default)
    {
        var http = _httpContextAccessor.HttpContext;
        var user = http?.User;

        var entry = new AuditLog
        {
            CreatedAtUtc = DateTime.UtcNow,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = user?.Identity?.Name,
            IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
            DetailsJson = details is null ? null : JsonSerializer.Serialize(details)
        };

        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync(ct);
    }
}
