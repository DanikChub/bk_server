namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Pages.Account;
using KV.Server.Localization;
using Volo.Abp.Account.Web.Pages.Account;

public class CustomResetPasswordConfirmationModel : ResetPasswordConfirmationModel
{
    public CustomResetPasswordConfirmationModel()
    {
        this.LocalizationResourceType = typeof(ServerResource);
    }
}
