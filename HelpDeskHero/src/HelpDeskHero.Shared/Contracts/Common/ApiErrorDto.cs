namespace HelpDeskHero.Shared.Contracts.Common;

public sealed class ApiErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public string TraceId { get; set; } = string.Empty;
}
