using HelpDeskHero.Api.Infrastructure.Persistence;
using HelpDeskHero.Shared.Contracts.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanViewAudit")]
public sealed class AuditController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogListItemDto>>> Get(
        [FromQuery] string? action,
        [FromQuery] string? entityName,
        [FromQuery] string? performedBy,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken ct)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action);

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(x => x.EntityName == entityName);

        if (!string.IsNullOrWhiteSpace(performedBy))
        {
            query = query.Where(x =>
                (x.UserName != null && x.UserName.Contains(performedBy)) ||
                (x.UserId != null && x.UserId.Contains(performedBy)));
        }

        if (fromUtc.HasValue)
            query = query.Where(x => x.CreatedAtUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(x => x.CreatedAtUtc <= toUtc.Value);

        var rows = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(200)
            .Select(x => new AuditLogListItemDto
            {
                CreatedAtUtc = x.CreatedAtUtc,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                PerformedBy = x.UserName ?? x.UserId ?? string.Empty,
                Details = x.DetailsJson ?? string.Empty
            })
            .ToListAsync(ct);

        return Ok(rows);
    }
}
