namespace KV.Server.Migration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

public class MigrationHostedService : IHostedService
{
    private readonly IAbpApplicationWithExternalServiceProvider _abpApplication;
    private readonly IServiceProvider _serviceProvider;
    private readonly KvMigrationService _kvMigrationService;

    public MigrationHostedService(KvMigrationService kvMigrationService,
        IServiceProvider serviceProvider,
        IAbpApplicationWithExternalServiceProvider abpApplication
        )
    {
        this._kvMigrationService = kvMigrationService;
        this._serviceProvider = serviceProvider;
        this._abpApplication = abpApplication;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this._abpApplication.Initialize(this._serviceProvider);

        await this._kvMigrationService.MigrateAsync();

    }

    public async Task StopAsync(CancellationToken cancellationToken) => await this._abpApplication.ShutdownAsync();
}
