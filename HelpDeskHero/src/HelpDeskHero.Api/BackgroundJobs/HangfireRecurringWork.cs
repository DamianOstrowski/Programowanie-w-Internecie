using HelpDeskHero.Api.Application.Interfaces;

namespace HelpDeskHero.Api.BackgroundJobs;

public sealed class HangfireRecurringWork
{
    private readonly IOutboxProcessor _outboxProcessor;
    private readonly ISlaMonitorService _slaMonitorService;

    public HangfireRecurringWork(IOutboxProcessor outboxProcessor, ISlaMonitorService slaMonitorService)
    {
        _outboxProcessor = outboxProcessor;
        _slaMonitorService = slaMonitorService;
    }

    public Task ProcessOutboxAsync() =>
        _outboxProcessor.ProcessPendingAsync(CancellationToken.None);

    public Task CheckSlaAsync() =>
        _slaMonitorService.CheckBreachesAsync(CancellationToken.None);
}
