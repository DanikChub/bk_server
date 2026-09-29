using System;
using System.Threading;
using System.Threading.Tasks;
using KV.Server.Web.BackgroundJobs.Args;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers.Hangfire;

namespace KV.Server.Web.BackgroundJobs;

public class UpdateCustomerStatusWorker : HangfireBackgroundWorkerBase
{
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly ILogger<UpdateCustomerStatusWorker> _logger;

    public UpdateCustomerStatusWorker(
        IBackgroundJobManager backgroundJobManager, 
        ILogger<UpdateCustomerStatusWorker> logger,
        IConfiguration configuration)
    {
        _backgroundJobManager = backgroundJobManager;
        _logger = logger;

        var cron = configuration["Workers:UpdateCustomerStatus:Cron"];

        if (string.IsNullOrEmpty(cron))
        {
            cron = "0 0 * * *";
        }

        RecurringJobId = nameof(UpdateCustomerStatusWorker);
        CronExpression = cron;
    }

    public override async Task DoWorkAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _backgroundJobManager.EnqueueAsync(new UpdateCustomerStatusArgs());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to do UpdateCustomerStatusWorker work");
        }
    }
}
