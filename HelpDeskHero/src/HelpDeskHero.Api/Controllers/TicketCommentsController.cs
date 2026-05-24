using HelpDeskHero.Api.Application;
using HelpDeskHero.Api.Application.Interfaces;
using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;
using HelpDeskHero.Api.Infrastructure.Services;
using HelpDeskHero.Shared.Contracts.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:int}/comments")]
[Authorize]
public sealed class TicketCommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    private readonly IOutboxWriter _outboxWriter;

    public TicketCommentsController(AppDbContext db, AuditService audit, IOutboxWriter outboxWriter)
    {
        _db = db;
        _audit = audit;
        _outboxWriter = outboxWriter;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketCommentDto>>> GetAll(int ticketId, CancellationToken ct)
    {
        var items = await _db.TicketComments
            .AsNoTracking()
            .Where(x => x.TicketId == ticketId)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new TicketCommentDto
            {
                Id = x.Id,
                TicketId = x.TicketId,
                Body = x.Body,
                IsInternal = x.IsInternal,
                CreatedAtUtc = x.CreatedAtUtc,
                CreatedByDisplayName = x.CreatedByDisplayName
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<TicketCommentDto>> Create(int ticketId, CreateTicketCommentDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Body))
            return BadRequest(new { code = "validation_error", message = "Comment body is required." });

        if (dto.Body.Trim().Length > 4000)
            return BadRequest(new { code = "validation_error", message = "Comment is too long." });

        var exists = await _db.Tickets.AnyAsync(x => x.Id == ticketId, ct);
        if (!exists)
            return NotFound();

        var entity = new TicketComment
        {
            TicketId = ticketId,
            Body = dto.Body.Trim(),
            IsInternal = dto.IsInternal,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown",
            CreatedByDisplayName = User.Identity?.Name ?? "unknown"
        };

        _db.TicketComments.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _audit.WriteAsync("CommentCreate", "TicketComment", entity.Id.ToString(), new { ticketId }, ct);

        var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ticketId, ct);
        if (ticket is not null)
        {
            await _outboxWriter.AddAsync("TicketChanged", TicketLiveUpdateFactory.Create(ticket, "CommentAdded"), ct);
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new TicketCommentDto
        {
            Id = entity.Id,
            TicketId = entity.TicketId,
            Body = entity.Body,
            IsInternal = entity.IsInternal,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedByDisplayName = entity.CreatedByDisplayName
        });
    }
}
