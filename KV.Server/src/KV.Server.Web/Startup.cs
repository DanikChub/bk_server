namespace KV.Server.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) => services.AddApplication<ServerWebModule>();

    public void Configure(IApplicationBuilder app) => app.InitializeApplication();
}
