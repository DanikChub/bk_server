namespace KV.Server.DbMigrator;

using System.Threading;
using System.Threading.Tasks;
using KV.Server.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Volo.Abp;

public class DbMigratorHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IConfiguration _configuration;

    public DbMigratorHostedService(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration)
    {
        this._hostApplicationLifetime = hostApplicationLifetime;
        this._configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var application = AbpApplicationFactory.Create<ServerDbMigratorModule>(options =>
        {
            options.Services.ReplaceConfiguration(this._configuration);
            options.UseAutofac();
            options.Services.AddLogging(c => c.AddSerilog());
        }))
        {
            application.Initialize();

            await application
                .ServiceProvider
                .GetRequiredService<ServerDbMigrationService>()
                .MigrateAsync();

            application.Shutdown();

            this._hostApplicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
