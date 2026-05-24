using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Dashboard;

namespace HelpDeskHero.UI.Services.Api;

public sealed class DashboardApiClient
{
    private readonly HttpClient _httpClient;

    public DashboardApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<DashboardSummaryDto?> GetSummaryAsync(CancellationToken ct = default) =>
        _httpClient.GetFromJsonAsync<DashboardSummaryDto>("api/dashboard/summary", ct);
}
