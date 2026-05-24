using HelpDeskHero.Shared.Contracts.Common;

namespace HelpDeskHero.UI.Services.Api;

public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public ApiErrorDto? Error { get; }

    public ApiException(string message, int statusCode, ApiErrorDto? error = null)
        : base(message)
    {
        StatusCode = statusCode;
        Error = error;
    }
}
