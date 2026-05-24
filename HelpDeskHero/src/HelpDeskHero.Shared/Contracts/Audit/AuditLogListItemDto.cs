namespace HelpDeskHero.Shared.Contracts.Audit;

public sealed class AuditLogListItemDto
{
    public DateTime CreatedAtUtc { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
