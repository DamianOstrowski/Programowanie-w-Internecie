namespace HelpDeskHero.Api.Domain;

public sealed class TicketSlaPolicy
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public int FirstResponseMinutes { get; set; }
    public int ResolveMinutes { get; set; }
    public bool IsActive { get; set; } = true;
}
