using HelpDeskHero.Api.Application.Interfaces;
using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHero.Api.Application.Services;

public sealed class TicketAssignmentService : ITicketAssignmentService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public TicketAssignmentService(UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<string?> AssignAsync(Ticket ticket, CancellationToken ct = default)
    {
        var agents = await _userManager.GetUsersInRoleAsync("Agent");
        if (agents.Count == 0)
            return null;

        var agentLoads = new List<(string UserId, int Count)>();

        foreach (var agent in agents)
        {
            var activeCount = await _db.Tickets.CountAsync(
                x => x.AssignedToUserId == agent.Id
                     && x.Status != "Closed"
                     && !x.IsDeleted,
                ct);

            agentLoads.Add((agent.Id, activeCount));
        }

        var selected = agentLoads.OrderBy(x => x.Count).First();
        ticket.AssignedToUserId = selected.UserId;

        return selected.UserId;
    }
}
