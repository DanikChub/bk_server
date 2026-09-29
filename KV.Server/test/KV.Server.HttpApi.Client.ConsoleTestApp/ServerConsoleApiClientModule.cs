namespace KV.Server.HttpApi.Client.ConsoleTestApp;

using System;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Volo.Abp.Http.Client;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

[DependsOn(
    typeof(ServerHttpApiClientModule),
    typeof(AbpHttpClientIdentityModelModule)
    )]
public class ServerConsoleApiClientModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context) => this.PreConfigure<AbpHttpClientBuilderOptions>(options => options.ProxyClientBuildActions.Add((remoteServiceName, clientBuilder) => clientBuilder.AddTransientHttpErrorPolicy(
                                                                                                           policyBuilder => policyBuilder.WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)))
                                                                                                       )));
}
