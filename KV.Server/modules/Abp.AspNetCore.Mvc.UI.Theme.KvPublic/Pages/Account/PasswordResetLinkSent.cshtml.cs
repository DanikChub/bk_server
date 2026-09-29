namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Pages.Account;
using KV.Server.Localization;
using Volo.Abp.Account.Web.Pages.Account;

public class CustomPasswordResetLinkSentModel : PasswordResetLinkSentModel
{
    public CustomPasswordResetLinkSentModel()
    {
        this.LocalizationResourceType = typeof(ServerResource);
    }
}
