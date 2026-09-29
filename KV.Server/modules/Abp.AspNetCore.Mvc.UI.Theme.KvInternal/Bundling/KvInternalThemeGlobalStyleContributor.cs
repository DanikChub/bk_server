namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Bundling;

using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

public class KvInternalThemeGlobalStyleContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        //context.Files.Add("/themes/kvinternal/assets/css/style.css");
        //context.Files.Add("/themes/kvinternal/assets/css/components.css");
        //context.Files.Add("/themes/kvinternal/assets/css/custom.css");
        //context.Files.Add("/libs/@fortawesome/fontawesome-free/css/all.css");

        context.Files.Add("/css/style.css");
        context.Files.Add("/css/animate.css");
        context.Files.Add("/css/global-styles.css");
    }
}
