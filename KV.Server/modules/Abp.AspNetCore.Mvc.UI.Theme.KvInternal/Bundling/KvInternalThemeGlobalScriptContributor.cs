namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Bundling;

using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

public class KvInternalThemeGlobalScriptContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        //context.Files.Add("/themes/kvinternal/assets/modules/tooltip.js");
        //context.Files.Add("/themes/kvinternal/assets/modules/nicescroll/jquery.nicescroll.js");
        //context.Files.Add("/themes/kvinternal/assets/modules/moment.min.js");
        //context.Files.Add("/themes/kvinternal/assets/js/kvinternal.js");
        //context.Files.Add("/themes/kvinternal/assets/js/scripts.js");
        //context.Files.Add("/themes/kvinternal/assets/js/custom.js");

        context.Files.Add("/js/popper.min.js");
        context.Files.Add("/js/plugins/metisMenu/jquery.metisMenu.js");
        context.Files.Add("/js/plugins/slimscroll/jquery.slimscroll.min.js");
        context.Files.Add("/js/plugins/flot/jquery.flot.js");
        context.Files.Add("/js/plugins/flot/jquery.flot.tooltip.min.js");
        context.Files.Add("/js/plugins/flot/jquery.flot.spline.js");
        context.Files.Add("/js/plugins/flot/jquery.flot.resize.js");
        context.Files.Add("/js/plugins/flot/jquery.flot.pie.js");
        context.Files.Add("/js/plugins/peity/jquery.peity.min.js");
        context.Files.Add("/js/demo/peity-demo.js");
        context.Files.Add("/js/inspinia.js");
        context.Files.Add("/js/plugins/pace/pace.min.js");
        context.Files.Add("/js/plugins/gritter/jquery.gritter.min.js");
        context.Files.Add("/js/plugins/sparkline/jquery.sparkline.min.js");
        context.Files.Add("/js/demo/sparkline-demo.js");
        context.Files.Add("/js/plugins/chartJs/Chart.min.js");
        context.Files.Add("/js/plugins/toastr/toastr.min.js");
    }
}
