using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Auth;

namespace HelpDeskHero.UI.Services.Auth;

public sealed class AuthHttpMessageHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    public AuthHttpMessageHandler(
        TokenStore tokenStore,
        IHttpClientFactory httpClientFactory,
        JwtAuthenticationStateProvider authStateProvider)
    {
        _tokenStore = tokenStore;
        _httpClientFactory = httpClientFactory;
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _tokenStore.GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);

            if (jwt.ValidTo <= DateTime.UtcNow.AddMinutes(1))
            {
                await TryRefreshAsync(cancellationToken);
                accessToken = await _tokenStore.GetAccessTokenAsync();
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshed = await TryRefreshAsync(cancellationToken);
            if (refreshed)
            {
                accessToken = await _tokenStore.GetAccessTokenAsync();

                var clone = await CloneRequestAsync(request);
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    clone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }

                response.Dispose();
                return await base.SendAsync(clone, cancellationToken);
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        var refreshToken = await _tokenStore.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        var client = _httpClientFactory.CreateClient("AnonymousApi");

        var result = await client.PostAsJsonAsync("api/auth/refresh", new RefreshRequestDto
        {
            RefreshToken = refreshToken,
            DeviceName = Environment.MachineName
        }, ct);

        if (!result.IsSuccessStatusCode)
        {
            await _tokenStore.ClearAsync();
            return false;
        }

        var dto = await result.Content.ReadFromJsonAsync<TokenResponseDto>(cancellationToken: ct);
        if (dto is null)
            return false;

        await _tokenStore.SetAccessTokenAsync(dto.AccessToken);
        await _tokenStore.SetRefreshTokenAsync(dto.RefreshToken);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(dto.AccessToken);
        _authStateProvider.NotifyUserAuthentication(jwt.Claims);

        return true;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
