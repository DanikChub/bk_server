namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal;

using Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Bundling;
using Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Menus;
using Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Toolbars;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AspNetCore.Mvc.UI.Theming;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;

[DependsOn(
    typeof(AbpAspNetCoreMvcUiThemeSharedModule),
    typeof(AbpAspNetCoreMvcUiMultiTenancyModule)
)]
public class AbpAspNetCoreMvcUiKvInternalThemeModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context) => this.PreConfigure<IMvcBuilder>(mvcBuilder => mvcBuilder.AddApplicationPartIfNotExists(typeof(AbpAspNetCoreMvcUiKvInternalThemeModule).Assembly));

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        this.Configure<AbpThemingOptions>(options =>
        {
            options.Themes.Add<KvInternalTheme>();

            options.DefaultThemeName ??= KvInternalTheme.Name;
        });

        this.Configure<AbpVirtualFileSystemOptions>(options => options.FileSets.AddEmbedded<AbpAspNetCoreMvcUiKvInternalThemeModule>("Abp.AspNetCore.Mvc.UI.Theme.KvInternal"));

        this.Configure<AbpToolbarOptions>(options => options.Contributors.Add(new KvInternalThemeMainTopToolbarContributor()));

        this.Configure<AbpBundlingOptions>(options =>
        {
            options
                .StyleBundles
                .Add(KvInternalThemeBundles.Styles.Global, bundle => bundle
                        .AddBaseBundles(StandardBundles.Styles.Global)
                        .AddContributors(typeof(KvInternalThemeGlobalStyleContributor)));

            options
                .ScriptBundles
                .Add(KvInternalThemeBundles.Scripts.Global, bundle => bundle
                        .AddBaseBundles(StandardBundles.Scripts.Global)
                        .AddContributors(typeof(KvInternalThemeGlobalScriptContributor)));
        });

        this.Configure<AbpNavigationOptions>(options => options.MenuContributors.Add(new KvInternalMenuContributor()));
    }
}
