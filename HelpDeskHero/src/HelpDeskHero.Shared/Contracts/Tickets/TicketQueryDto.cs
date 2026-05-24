namespace HelpDeskHero.Shared.Contracts.Tickets;

public sealed class TicketQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }

    public string SortBy { get; set; } = "CreatedAtUtc";
    public bool Desc { get; set; } = true;
}
