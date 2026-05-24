using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;
using HelpDeskHero.Api.Infrastructure.Services;
using HelpDeskHero.Api.Infrastructure.Storage;
using HelpDeskHero.Shared.Contracts.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDeskHero.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:int}/attachments")]
[Authorize]
public sealed class TicketAttachmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IFileStorage _storage;
    private readonly AuditService _audit;

    public TicketAttachmentsController(AppDbContext db, IFileStorage storage, AuditService audit)
    {
        _db = db;
        _storage = storage;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketAttachmentDto>>> GetAll(int ticketId, CancellationToken ct)
    {
        var items = await _db.TicketAttachments
            .AsNoTracking()
            .Where(x => x.TicketId == ticketId)
            .OrderByDescending(x => x.UploadedAtUtc)
            .Select(x => new TicketAttachmentDto
            {
                Id = x.Id,
                TicketId = x.TicketId,
                OriginalFileName = x.OriginalFileName,
                ContentType = x.ContentType,
                SizeBytes = x.SizeBytes,
                UploadedAtUtc = x.UploadedAtUtc,
                UploadedByUserId = x.UploadedByUserId
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<TicketAttachmentDto>> Upload(int ticketId, [FromForm] IFormFile file, CancellationToken ct)
    {
        var exists = await _db.Tickets.AnyAsync(x => x.Id == ticketId, ct);
        if (!exists)
            return NotFound();

        try
        {
            AttachmentValidation.Validate(file);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { code = "validation_error", message = ex.Message });
        }

        var stored = await _storage.SaveAsync(file, ct);

        var entity = new TicketAttachment
        {
            TicketId = ticketId,
            OriginalFileName = stored.OriginalFileName,
            StoredFileName = stored.StoredFileName,
            RelativePath = stored.RelativePath,
            ContentType = stored.ContentType,
            SizeBytes = stored.SizeBytes,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown"
        };

        _db.TicketAttachments.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _audit.WriteAsync("AttachmentUpload", "TicketAttachment", entity.Id.ToString(), new { ticketId }, ct);

        return Ok(new TicketAttachmentDto
        {
            Id = entity.Id,
            TicketId = entity.TicketId,
            OriginalFileName = entity.OriginalFileName,
            ContentType = entity.ContentType,
            SizeBytes = entity.SizeBytes,
            UploadedAtUtc = entity.UploadedAtUtc,
            UploadedByUserId = entity.UploadedByUserId
        });
    }

    [HttpGet("{attachmentId:int}/download")]
    public async Task<IActionResult> Download(int ticketId, int attachmentId, CancellationToken ct)
    {
        var item = await _db.TicketAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == attachmentId && x.TicketId == ticketId, ct);

        if (item is null)
            return NotFound();

        var stream = await _storage.OpenReadAsync(item.RelativePath, ct);
        await _audit.WriteAsync("AttachmentDownload", "TicketAttachment", item.Id.ToString(), new { ticketId }, ct);
        return File(stream, item.ContentType, item.OriginalFileName);
    }
}
