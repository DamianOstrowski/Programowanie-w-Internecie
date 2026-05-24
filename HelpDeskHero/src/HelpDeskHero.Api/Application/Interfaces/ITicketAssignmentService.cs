using HelpDeskHero.Api.Domain;

namespace HelpDeskHero.Api.Application.Interfaces;

public interface ITicketAssignmentService
{
    Task<string?> AssignAsync(Ticket ticket, CancellationToken ct = default);
}
