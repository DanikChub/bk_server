namespace KV.Server;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ServerWebTestStartup
{
    public static void ConfigureServices(IServiceCollection services) => services.AddApplication<ServerWebTestModule>();

    public static void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory) => app.InitializeApplication();
}
