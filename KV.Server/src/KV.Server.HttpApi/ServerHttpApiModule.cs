namespace KV.Server;

using global::Localization.Resources.AbpUi;
using KV.Server.Localization;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

[DependsOn(
    typeof(ServerApplicationContractsModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpTenantManagementHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule)
    )]
public class ServerHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context) => this.ConfigureLocalization();

    private void ConfigureLocalization() => this.Configure<AbpLocalizationOptions>(options => options.Resources
                                                             .Get<ServerResource>()
                                                             .AddBaseTypes(
                                                                 typeof(AbpUiResource)
                                                             ));
}
