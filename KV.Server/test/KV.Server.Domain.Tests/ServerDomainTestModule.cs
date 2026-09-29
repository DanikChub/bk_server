namespace KV.Server;

using KV.Server.EntityFrameworkCore;
using Volo.Abp.Modularity;

[DependsOn(
    typeof(ServerEntityFrameworkCoreTestModule)
    )]
public class ServerDomainTestModule : AbpModule
{

}
