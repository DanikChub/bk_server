
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace KV.Server.Tenants;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

public class TenantProfileSeederContributor : ITransientDependency, IDataSeedContributor
{
    public TenantProfileSeederContributor() { }

    public async Task SeedAsync(DataSeedContext context)
    {
        // seed tenant profile data here
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
