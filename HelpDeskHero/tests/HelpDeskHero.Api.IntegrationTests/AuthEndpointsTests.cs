using System.Net;
using System.Net.Http.Json;
using HelpDeskHero.Shared.Contracts.Auth;

namespace HelpDeskHero.Api.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RevokeAllSessions_ShouldRevokeRefreshTokens()
    {
        var anon = _factory.CreateClient();
        var login = await anon.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequestDto
            {
                UserName = "agent",
                Password = "Agent1234",
                DeviceName = "device-a"
            });
        login.EnsureSuccessStatusCode();
        var tokens = await login.Content.ReadFromJsonAsync<TokenResponseDto>();
        tokens.Should().NotBeNull();

        var authed = _factory.CreateClient();
        authed.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var revoke = await authed.PostAsync("/api/auth/revoke-all", null);
        revoke.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refresh = await anon.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshRequestDto { RefreshToken = tokens.RefreshToken, DeviceName = "device-a" });
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
