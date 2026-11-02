using Microsoft.EntityFrameworkCore;

namespace KV.Server.EntityFrameworkCore
{
    // Schema-only context. It never reads legacy application configuration or opens a connection.
    public class ServerDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseNpgsql("Host=localhost;Database=schema_export_unused;Username=unused");

        protected override void OnModelCreating(ModelBuilder builder) =>
            new KV.Server.Migrations.ServerDbContextModelSnapshot().Apply(builder);
    }
}

namespace KV.Server.Migrations
{
    partial class ServerDbContextModelSnapshot
    {
        public void Apply(ModelBuilder builder) => BuildModel(builder);
    }
}

namespace Volo.Abp.EntityFrameworkCore
{
    // The saved model references this enum only in the _Abp_DatabaseProvider annotation.
    // It has no effect on PostgreSQL SQL generation. No ABP services or business entities
    // are reimplemented here; all columns, constraints and data come from original sources.
    public enum EfCoreDatabaseProvider { PostgreSql }
}
