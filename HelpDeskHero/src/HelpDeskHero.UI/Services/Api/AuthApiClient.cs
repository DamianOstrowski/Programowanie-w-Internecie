using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Auth;
using HelpDeskHero.UI.Services.Auth;

namespace HelpDeskHero.UI.Services.Api;

public sealed class AuthApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    public AuthApiClient(
        IHttpClientFactory httpClientFactory,
        TokenStore tokenStore,
        JwtAuthenticationStateProvider authStateProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("AnonymousApi");
        var response = await client.PostAsJsonAsync("api/auth/login", dto, ct);

        if (!response.IsSuccessStatusCode)
            return false;

        var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>(cancellationToken: ct);
        if (token is null)
            return false;

        await _tokenStore.SetAccessTokenAsync(token.AccessToken);
        await _tokenStore.SetRefreshTokenAsync(token.RefreshToken);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        _authStateProvider.NotifyUserAuthentication(jwt.Claims);

        return true;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var refresh = await _tokenStore.GetRefreshTokenAsync();

        if (!string.IsNullOrWhiteSpace(refresh))
        {
            await client.PostAsJsonAsync("api/auth/logout", new RefreshRequestDto
            {
                RefreshToken = refresh,
                DeviceName = Environment.MachineName
            }, ct);
        }

        await _tokenStore.ClearAsync();
        _authStateProvider.NotifyUserLogout();
    }

    public async Task<bool> RevokeAllSessionsAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsync("api/auth/revoke-all", null, ct);
        return response.IsSuccessStatusCode;
    }
}
