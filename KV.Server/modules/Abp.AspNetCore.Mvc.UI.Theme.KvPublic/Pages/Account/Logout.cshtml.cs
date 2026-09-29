namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Pages.Account;
using KV.Server.Localization;
using Volo.Abp.Account.Web.Pages.Account;

public class CustomLogoutModel : LogoutModel
{
    public CustomLogoutModel()
    {
        this.LocalizationResourceType = typeof(ServerResource);
    }
}
