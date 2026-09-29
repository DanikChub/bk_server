namespace KV.Server.Data;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

/* This is used if database provider does't define
 * IServerDbSchemaMigrator implementation.
 */
public class NullServerDbSchemaMigrator : IServerDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync() => Task.CompletedTask;
}
