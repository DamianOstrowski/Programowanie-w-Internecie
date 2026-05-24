using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Tickets;
using Microsoft.AspNetCore.Components.Forms;

namespace HelpDeskHero.UI.Services.Api;

public sealed class TicketAttachmentApiClient
{
    private readonly HttpClient _http;

    public TicketAttachmentApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<TicketAttachmentDto>> GetAllAsync(int ticketId, CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<TicketAttachmentDto>>(
            $"api/tickets/{ticketId}/attachments", ct);

        return result ?? [];
    }

    public async Task<TicketAttachmentDto?> UploadAsync(
        int ticketId,
        IBrowserFile file,
        long maxFileSize = 10 * 1024 * 1024,
        CancellationToken ct = default)
    {
        await using var stream = file.OpenReadStream(maxFileSize, ct);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var response = await _http.PostAsync($"api/tickets/{ticketId}/attachments", content, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TicketAttachmentDto>(cancellationToken: ct);
    }
}
