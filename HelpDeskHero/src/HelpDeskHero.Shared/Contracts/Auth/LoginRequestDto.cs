using System.ComponentModel.DataAnnotations;

namespace HelpDeskHero.Shared.Contracts.Auth;

public sealed class LoginRequestDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string DeviceName { get; set; } = "Unknown";
}
