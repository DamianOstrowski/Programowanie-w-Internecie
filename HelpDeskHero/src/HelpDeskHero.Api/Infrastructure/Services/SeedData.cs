using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDeskHero.Api.Infrastructure.Services;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedTicketSlaPoliciesAsync(db);

        string[] roles = ["Admin", "Agent", "User"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminUserName = "admin";
        var admin = await userManager.FindByNameAsync(adminUserName);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminUserName,
                Email = "admin@helpdeskhero.local",
                DisplayName = "System Admin",
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, "Admin1234");
            if (createResult.Succeeded)
            {
                await userManager.AddToRolesAsync(admin, ["Admin", "Agent"]);
            }
        }

        const string agentUserName = "agent";
        var agent = await userManager.FindByNameAsync(agentUserName);

        if (agent is null)
        {
            agent = new ApplicationUser
            {
                UserName = agentUserName,
                Email = "agent@helpdeskhero.local",
                DisplayName = "Support Agent",
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(agent, "Agent1234");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(agent, "Agent");
            }
        }

        const string endUserName = "user";
        var endUser = await userManager.FindByNameAsync(endUserName);

        if (endUser is null)
        {
            endUser = new ApplicationUser
            {
                UserName = endUserName,
                Email = "user@helpdeskhero.local",
                DisplayName = "End User",
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(endUser, "User1234");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(endUser, "User");
            }
        }
    }

    private static async Task SeedTicketSlaPoliciesAsync(AppDbContext db)
    {
        if (await db.TicketSlaPolicies.AnyAsync())
            return;

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
}
