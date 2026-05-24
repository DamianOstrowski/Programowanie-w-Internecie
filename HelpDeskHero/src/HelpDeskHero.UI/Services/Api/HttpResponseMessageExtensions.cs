using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Common;

namespace HelpDeskHero.UI.Services.Api;

public static class HttpResponseMessageExtensions
{
    public static async Task EnsureSuccessOrThrowAsync(this HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        ApiErrorDto? err = null;
        try
        {
            err = await response.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken: ct);
        }
        catch
        {
            // ignore
        }

        var message = err?.Message ?? response.ReasonPhrase ?? "Zadanie HTTP nie powiodlo sie.";
        throw new ApiException(message, (int)response.StatusCode, err);
    }
}
