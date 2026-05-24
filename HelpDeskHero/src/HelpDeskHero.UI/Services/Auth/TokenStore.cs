using Microsoft.JSInterop;

namespace HelpDeskHero.UI.Services.Auth;

public sealed class TokenStore
{
    private readonly IJSRuntime _js;

    public TokenStore(IJSRuntime js)
    {
        _js = js;
    }

    public Task SetAccessTokenAsync(string token) =>
        _js.InvokeVoidAsync("localStorage.setItem", "auth.access_token", token).AsTask();

    public Task SetRefreshTokenAsync(string token) =>
        _js.InvokeVoidAsync("localStorage.setItem", "auth.refresh_token", token).AsTask();

    public async Task<string?> GetAccessTokenAsync() =>
        await _js.InvokeAsync<string?>("localStorage.getItem", "auth.access_token");

    public async Task<string?> GetRefreshTokenAsync() =>
        await _js.InvokeAsync<string?>("localStorage.getItem", "auth.refresh_token");

    public async Task ClearAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "auth.access_token");
        await _js.InvokeVoidAsync("localStorage.removeItem", "auth.refresh_token");
    }
}
