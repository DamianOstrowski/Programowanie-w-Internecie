using System.ComponentModel.DataAnnotations;

namespace HelpDeskHero.Shared.Contracts.Tickets;

public sealed class CreateTicketDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = "Medium";
}
