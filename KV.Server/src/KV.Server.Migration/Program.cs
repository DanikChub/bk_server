namespace KV.Server.Migration;
using System;
using System.Threading.Tasks;
using KV.Server.Migration.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
#if DEBUG
                .MinimumLevel.Override("KV.Server", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("KV.Server", LogEventLevel.Information)
#endif
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File("Logs/logs.txt"))
                .WriteTo.Async(c => c.Console())
                .CreateLogger();

        try
        {
            Log.Information("Starting console host.");

            var builder = Host.CreateDefaultBuilder(args);

            builder
                .ConfigureAppConfiguration(build =>
                {
                    build.AddUserSecrets<Program>();
                    build.AddJsonFile("appsettings.secrets.json", optional: true);
                })
                .ConfigureLogging((context, logging) => logging.ClearProviders())
                .ConfigureServices(services =>
            {
                services.AddHostedService<MigrationHostedService>();
                services.AddApplicationAsync<MigrationModule>(context =>
                {
                    var configuration = services.GetConfiguration();
                    context.Services.ReplaceConfiguration(configuration);
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                    context.Services.Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
                    context.Services.AddDbContext<hdContentContext>(options => options.UseSqlServer(configuration.GetConnectionString("OldDatabase")));
                });
            }).UseAutofac().UseConsoleLifetime();

            var host = builder.Build();
            await host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>().InitializeAsync(host.Services);

            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
