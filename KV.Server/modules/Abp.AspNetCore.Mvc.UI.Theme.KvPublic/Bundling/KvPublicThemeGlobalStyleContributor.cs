namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Bundling;

using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

public class KvPublicThemeGlobalStyleContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        //context.Files.Add("/themes/kvpublic/assets/css/style.css");
        //context.Files.Add("/themes/kvpublic/assets/css/components.css");
        //context.Files.Add("/themes/kvpublic/assets/css/custom.css");
        //context.Files.Add("/libs/@fortawesome/fontawesome-free/css/all.css");

        context.Files.Add("/css/style.css");
        context.Files.Add("/css/animate.css");
        context.Files.Add("/css/global-styles.css");
    }
}
