namespace HelpDeskHero.Shared.Contracts.Auth;

public sealed class RefreshRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
    public string DeviceName { get; set; } = "Unknown";
}
