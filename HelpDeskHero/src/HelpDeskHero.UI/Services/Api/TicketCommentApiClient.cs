using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Tickets;

namespace HelpDeskHero.UI.Services.Api;

public sealed class TicketCommentApiClient
{
    private readonly HttpClient _http;

    public TicketCommentApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<TicketCommentDto>> GetAllAsync(int ticketId, CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<TicketCommentDto>>(
            $"api/tickets/{ticketId}/comments", ct);

        return result ?? [];
    }

    public async Task<TicketCommentDto?> CreateAsync(int ticketId, CreateTicketCommentDto dto, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"api/tickets/{ticketId}/comments", dto, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TicketCommentDto>(cancellationToken: ct);
    }
}
