namespace KV.Server;

using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

public class ServerTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public Task SeedAsync(DataSeedContext context) =>
        /* Seed additional test data... */

        Task.CompletedTask;
}
