namespace KV.Server.Web.Pages;
using KV.Server.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

/* Inherit your PageModel classes from this class.
 */
public abstract class ServerPageModel : AbpPageModel
{
    protected ServerPageModel()
    {
        this.LocalizationResourceType = typeof(ServerResource);
    }
}
