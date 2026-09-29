namespace KV.Server.HttpApi.Client.ConsoleTestApp;

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal sealed class Program
{
    private static async Task Main(string[] args) => await CreateHostBuilder(args).RunConsoleAsync();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(build => build.AddJsonFile("appsettings.secrets.json", optional: true))
            .ConfigureServices((hostContext, services) => services.AddHostedService<ConsoleTestAppHostedService>());
}
