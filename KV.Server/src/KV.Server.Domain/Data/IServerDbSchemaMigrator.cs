namespace KV.Server.Data;
using System.Threading.Tasks;

public interface IServerDbSchemaMigrator
{
    Task MigrateAsync();
}
