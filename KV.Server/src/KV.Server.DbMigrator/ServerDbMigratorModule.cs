namespace KV.Server.DbMigrator;

using System;
using KV.Server.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(ServerEntityFrameworkCoreModule),
    typeof(ServerApplicationContractsModule)
    )]
public class ServerDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        this.Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        this.Configure<DbContextOptionsBuilder>(builder => 
        {
            builder.UseNpgsql(
                context.Services.GetConfiguration().GetConnectionString("Default"), 
                options =>
                    {
                        options.CommandTimeout(180);
                    });
            builder.EnableSensitiveDataLogging(true);
            builder.EnableDetailedErrors(true);
        });
    }
}
