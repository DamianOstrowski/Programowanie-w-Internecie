using HelpDeskHero.Api.Infrastructure.Persistence;
using HelpDeskHero.Shared.Contracts.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken ct)
    {
        var dto = new DashboardSummaryDto
        {
            TotalTickets = await _db.Tickets.IgnoreQueryFilters().CountAsync(ct),
            OpenTickets = await _db.Tickets.CountAsync(x => x.Status != "Closed", ct),
            ClosedTickets = await _db.Tickets.CountAsync(x => x.Status == "Closed", ct),
            DeletedTickets = await _db.Tickets.IgnoreQueryFilters().CountAsync(x => x.IsDeleted, ct),
            HighPriorityOpenTickets = await _db.Tickets.CountAsync(x => x.Priority == "High" && x.Status != "Closed", ct),
            RecentAuditItems = await _db.AuditLogs
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(10)
                .Select(x => new RecentAuditItemDto
                {
                    CreatedAtUtc = x.CreatedAtUtc,
                    Action = x.Action,
                    EntityName = x.EntityName,
                    PerformedBy = x.UserName ?? x.UserId ?? string.Empty
                })
                .ToListAsync(ct)
        };

        return Ok(dto);
    }
}
