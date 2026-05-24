namespace HelpDeskHero.Api.Domain;

public sealed class AuditLog
{
    public long Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }

    public string? DetailsJson { get; set; }
}
