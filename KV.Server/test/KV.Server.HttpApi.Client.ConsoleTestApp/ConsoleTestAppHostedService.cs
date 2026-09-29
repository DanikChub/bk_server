namespace KV.Server.HttpApi.Client.ConsoleTestApp;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

public class ConsoleTestAppHostedService : IHostedService
{
    private readonly IConfiguration _configuration;

    public ConsoleTestAppHostedService(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var application = AbpApplicationFactory.Create<ServerConsoleApiClientModule>(options => options.Services.ReplaceConfiguration(this._configuration)))
        {
            application.Initialize();

            var demo = application.ServiceProvider.GetRequiredService<ClientDemoService>();
            await demo.RunAsync();

            application.Shutdown();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
