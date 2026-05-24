using Hangfire;
using HelpDeskHero.Api.BackgroundJobs.Contracts;

namespace HelpDeskHero.Api.Infrastructure.Services;

public sealed class HangfireNotificationQueue : INotificationQueue
{
    private readonly IBackgroundJobClient _jobs;

    public HangfireNotificationQueue(IBackgroundJobClient jobs)
    {
        _jobs = jobs;
    }

    public void EnqueueTicketCreated(int ticketId)
    {
        _jobs.Enqueue<INotificationJob>(job =>
            job.SendTicketCreatedNotificationsAsync(ticketId, CancellationToken.None));
    }
}
