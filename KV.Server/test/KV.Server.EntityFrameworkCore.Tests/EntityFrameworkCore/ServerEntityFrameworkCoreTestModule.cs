namespace KV.Server.EntityFrameworkCore;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

[DependsOn(
    typeof(ServerEntityFrameworkCoreModule),
    typeof(ServerTestBaseModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
    )]
public class ServerEntityFrameworkCoreTestModule : AbpModule
{
    private SqliteConnection _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context) => this.ConfigureInMemorySqlite(context.Services);

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        this._sqliteConnection = CreateDatabaseAndGetConnection();

        services.Configure<AbpDbContextOptions>(options => options.Configure(context => context.DbContextOptions.UseSqlite(this._sqliteConnection)));
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context) => this._sqliteConnection.Dispose();

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ServerDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new ServerDbContext(options))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        return connection;
    }
}
