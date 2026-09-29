namespace KV.Server;
using KV.Server.Localization;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule),
    typeof(AbpBlobStoringModule))]
public class ServerDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        ServerGlobalFeatureConfigurator.Configure();
        ServerModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        this.Configure<AbpVirtualFileSystemOptions>(options => options.FileSets.AddEmbedded<ServerDomainSharedModule>());

        this.Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<ServerResource>("ru")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddBaseTypes(typeof(IdentityResource))
                .AddVirtualJson("/Localization/Server");

            options.DefaultResourceType = typeof(ServerResource);
        });

        this.Configure<AbpExceptionLocalizationOptions>(options => options.MapCodeNamespace("Server", typeof(ServerResource)));
    }
}
