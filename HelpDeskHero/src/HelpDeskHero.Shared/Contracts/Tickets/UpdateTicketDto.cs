using System.ComponentModel.DataAnnotations;

namespace HelpDeskHero.Shared.Contracts.Tickets;

public sealed class UpdateTicketDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [RegularExpression("New|InProgress|Resolved|Closed")]
    public string Status { get; set; } = "New";

    [Required]
    [RegularExpression("Low|Medium|High|Critical")]
    public string Priority { get; set; } = "Medium";

    [Required]
    public string RowVersionBase64 { get; set; } = string.Empty;
}
