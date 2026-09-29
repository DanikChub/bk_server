namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic;

using Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Bundling;
using Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Menus;
using Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Toolbars;
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
public class AbpAspNetCoreMvcUiKvPublicThemeModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context) => this.PreConfigure<IMvcBuilder>(mvcBuilder => mvcBuilder.AddApplicationPartIfNotExists(typeof(AbpAspNetCoreMvcUiKvPublicThemeModule).Assembly));

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        this.Configure<AbpThemingOptions>(options =>
        {
            options.Themes.Add<KvPublicTheme>();

            options.DefaultThemeName ??= KvPublicTheme.Name;
        });

        this.Configure<AbpVirtualFileSystemOptions>(options => options.FileSets.AddEmbedded<AbpAspNetCoreMvcUiKvPublicThemeModule>("Abp.AspNetCore.Mvc.UI.Theme.KvPublic"));

        this.Configure<AbpToolbarOptions>(options => options.Contributors.Add(new KvPublicThemeMainTopToolbarContributor()));

        this.Configure<AbpBundlingOptions>(options =>
        {
            options
                .StyleBundles
                .Add(KvPublicThemeBundles.Styles.Global, bundle => bundle
                        .AddBaseBundles(StandardBundles.Styles.Global)
                        .AddContributors(typeof(KvPublicThemeGlobalStyleContributor)));

            options
                .ScriptBundles
                .Add(KvPublicThemeBundles.Scripts.Global, bundle => bundle
                        .AddBaseBundles(StandardBundles.Scripts.Global)
                        .AddContributors(typeof(KvPublicThemeGlobalScriptContributor)));
        });

        this.Configure<AbpNavigationOptions>(options => options.MenuContributors.Add(new KvPublicMenuContributor()));
    }
}
