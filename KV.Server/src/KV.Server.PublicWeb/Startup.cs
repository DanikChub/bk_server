namespace KV.Server.PublicWeb;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) => services.AddApplication<ServerPublicWebModule>();

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory) => app.InitializeApplication();
}
