using System.Net;
using System.Security.Claims;
using Bunit;
using HelpDeskHero.Shared.Contracts.Common;
using HelpDeskHero.Shared.Contracts.Tickets;
using HelpDeskHero.UI.Pages.Tickets;
using HelpDeskHero.UI.Services.Api;
using HelpDeskHero.UI.Services.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDeskHero.UI.Tests;

public sealed class TicketListPageTests : TestContext
{
    public TicketListPageTests()
    {
        Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("CanManageTickets", p => p.RequireRole("Admin", "Agent"));
            options.AddPolicy("CanViewAudit", p => p.RequireRole("Admin"));
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            options.AddPolicy("AgentOrAdmin", p => p.RequireRole("Agent", "Admin"));
        });
        Services.AddSingleton<AuthenticationStateProvider>(_ => new TestAuthStateProvider());
        Services.AddSingleton<IAuthorizationService, AllowAllAuthorizationService>();
        Services.AddSingleton<ITicketApiClient>(new FakeTicketApiClient());
        Services.AddSingleton<ITicketRealtime>(new NoOpTicketRealtime());
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private sealed class AllowAllAuthorizationService : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements) =>
            Task.FromResult(AuthorizationResult.Success());

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, AuthorizationPolicy policy) =>
            Task.FromResult(AuthorizationResult.Success());

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName) =>
            Task.FromResult(AuthorizationResult.Success());
    }

    [Fact]
    public void TicketListPage_ShouldRenderTicketTitle()
    {
        var cut = Render(builder =>
        {
            builder.OpenComponent<CascadingAuthenticationState>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(inner =>
            {
                inner.OpenComponent<TicketListPage>(0);
                inner.CloseComponent();
            }));
            builder.CloseComponent();
        });

        cut.Markup.Should().Contain("Test Ticket from bUnit");
    }

    private sealed class TestAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "agent"),
                new Claim(ClaimTypes.Role, "Agent")
            };
            var id = new ClaimsIdentity(claims, "test");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(id)));
        }
    }

    private sealed class FakeTicketApiClient : ITicketApiClient
    {
        public Task<PagedResultDto<TicketDto>?> GetPageAsync(TicketQueryDto query, CancellationToken ct = default) =>
            Task.FromResult<PagedResultDto<TicketDto>?>(new PagedResultDto<TicketDto>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1,
                Items =
                [
                    new TicketDto
                    {
                        Id = 1,
                        Number = "HDH-0001",
                        Title = "Test Ticket from bUnit",
                        Description = "desc",
                        Status = "New",
                        Priority = "High",
                        CreatedAtUtc = DateTime.UtcNow,
                        RowVersionBase64 = Convert.ToBase64String(new byte[8])
                    }
                ]
            });

        public Task<TicketDto?> GetByIdAsync(int id, CancellationToken ct = default) => Task.FromResult<TicketDto?>(null);

        public Task<HttpResponseMessage> CreateAsync(CreateTicketDto dto, CancellationToken ct = default) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> UpdateAsync(int id, UpdateTicketDto dto, CancellationToken ct = default) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> ExportCsvAsync(string? status, string? priority, CancellationToken ct = default) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<List<TicketDto>?> GetDeletedAsync(CancellationToken ct = default) =>
            Task.FromResult<List<TicketDto>?>([]);

        public Task<HttpResponseMessage> RestoreAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
