namespace KV.Server.PublicWeb.Pages;
using KV.Server.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

public abstract class ServerPageModel : AbpPageModel
{
    protected ServerPageModel()
    {
        this.LocalizationResourceType = typeof(ServerResource);
    }
}
