using System.Net.Http.Json;
using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDeskHero.Api.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SemaphoreSlim _seedLock = new(1, 1);
    private bool _databaseInitialized;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("TestDatabaseName", Guid.NewGuid().ToString("N"));
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _seedLock.WaitAsync();
        try
        {
            if (_databaseInitialized)
                return;

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { "Admin", "Agent", "User" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var admin = new ApplicationUser
            {
                UserName = "admin",
                DisplayName = "Admin",
                IsActive = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin1234");
            await userManager.AddToRolesAsync(admin, ["Admin", "Agent"]);

            var agent = new ApplicationUser
            {
                UserName = "agent",
                DisplayName = "Agent",
                IsActive = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(agent, "Agent1234");
            await userManager.AddToRoleAsync(agent, "Agent");

            if (!await db.TicketSlaPolicies.AnyAsync())
            {
                db.TicketSlaPolicies.AddRange(
                    new TicketSlaPolicy
                    {
                        Name = "SLA Low",
                        Priority = "Low",
                        FirstResponseMinutes = 480,
                        ResolveMinutes = 2880,
                        IsActive = true
                    },
                    new TicketSlaPolicy
                    {
                        Name = "SLA Medium",
                        Priority = "Medium",
                        FirstResponseMinutes = 240,
                        ResolveMinutes = 1440,
                        IsActive = true
                    },
                    new TicketSlaPolicy
                    {
                        Name = "SLA High",
                        Priority = "High",
                        FirstResponseMinutes = 60,
                        ResolveMinutes = 480,
                        IsActive = true
                    },
                    new TicketSlaPolicy
                    {
                        Name = "SLA Critical",
                        Priority = "Critical",
                        FirstResponseMinutes = 15,
                        ResolveMinutes = 120,
                        IsActive = true
                    });
                await db.SaveChangesAsync();
            }

            _databaseInitialized = true;
        }
        finally
        {
            _seedLock.Release();
        }
    }

    Task IAsyncLifetime.DisposeAsync() => base.DisposeAsync().AsTask();

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string userName, string password)
    {
        var client = CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new HelpDeskHero.Shared.Contracts.Auth.LoginRequestDto
            {
                UserName = userName,
                Password = password,
                DeviceName = "integration-test"
            });

        loginResponse.EnsureSuccessStatusCode();
        var token = await loginResponse.Content.ReadFromJsonAsync<HelpDeskHero.Shared.Contracts.Auth.TokenResponseDto>()
            ?? throw new InvalidOperationException("Login returned no token.");

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

        return client;
    }
}
