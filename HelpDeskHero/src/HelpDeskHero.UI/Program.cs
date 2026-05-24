using Blazored.LocalStorage;
using HelpDeskHero.UI;
using HelpDeskHero.UI.Services.Api;
using HelpDeskHero.UI.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:5001";

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("CanManageTickets", policy => policy.RequireRole("Admin", "Agent", "User"));
});
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

// Rejestracja handlera autoryzacji
builder.Services.AddScoped<AuthHttpMessageHandler>();

// To naprawia błąd z IHttpClientFactory w konsoli przeglądarki
builder.Services.AddHttpClient("AnonymousApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthHttpMessageHandler>();

// Domyślny wstrzykiwany HttpClient będzie tym zabezpieczonym ("Api")
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<TicketApiClient>();
// Podstawowi klienci
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<TicketApiClient>();

// Rejestracja klientów API - mapowanie Interfejsów na Klasy
builder.Services.AddScoped<HelpDeskHero.UI.Services.Api.AuthApiClient, HelpDeskHero.UI.Services.Api.AuthApiClient>();
builder.Services.AddScoped<HelpDeskHero.UI.Services.Api.ITicketApiClient, HelpDeskHero.UI.Services.Api.TicketApiClient>();
builder.Services.AddScoped<HelpDeskHero.UI.Services.Api.DashboardApiClient, HelpDeskHero.UI.Services.Api.DashboardApiClient>();
builder.Services.AddScoped<HelpDeskHero.UI.Services.Api.NotificationApiClient, HelpDeskHero.UI.Services.Api.NotificationApiClient>();
builder.Services.AddScoped<HelpDeskHero.UI.Services.Api.TicketCommentApiClient, HelpDeskHero.UI.Services.Api.TicketCommentApiClient>();
builder.Services.AddScoped<HelpDeskHero.UI.Services.Api.TicketAttachmentApiClient, HelpDeskHero.UI.Services.Api.TicketAttachmentApiClient>();

// Twój poprawnie zarejestrowany klient SignalR
builder.Services.AddScoped<HelpDeskHero.UI.Services.Realtime.ITicketRealtime, HelpDeskHero.UI.Services.Realtime.TicketSignalRRealtime>();

await builder.Build().RunAsync();