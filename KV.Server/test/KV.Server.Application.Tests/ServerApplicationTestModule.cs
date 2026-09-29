namespace KV.Server;

using Volo.Abp.Modularity;

[DependsOn(
    typeof(ServerApplicationModule),
    typeof(ServerDomainTestModule)
    )]
public class ServerApplicationTestModule : AbpModule
{

}
