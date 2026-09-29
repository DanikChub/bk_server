namespace KV.Server.EntityFrameworkCore;

using System;
using System.Threading.Tasks;
using KV.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;

public class EntityFrameworkCoreServerDbSchemaMigrator
    : IServerDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreServerDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the ServerDbContext
        * from IServiceProvider (instead of directly injecting it)
        * to properly get the connection string of the current tenant in the
        * current scope.
        */

        var database = this._serviceProvider
            .GetRequiredService<ServerDbContext>()
            .Database;
        database.SetCommandTimeout(TimeSpan.FromSeconds(600));
        await database
            .MigrateAsync();
    }
}
