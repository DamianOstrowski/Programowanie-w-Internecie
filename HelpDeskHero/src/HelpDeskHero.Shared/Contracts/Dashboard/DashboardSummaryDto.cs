namespace HelpDeskHero.Shared.Contracts.Dashboard;

public sealed class DashboardSummaryDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int DeletedTickets { get; set; }
    public int HighPriorityOpenTickets { get; set; }
    public List<RecentAuditItemDto> RecentAuditItems { get; set; } = new();
}

public sealed class RecentAuditItemDto
{
    public DateTime CreatedAtUtc { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
}
