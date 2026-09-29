namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Pages.Account;
using KV.Server.Localization;
using Microsoft.Extensions.Options;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Account.Web.ProfileManagement;

public class CustomManageModel : ManageModel
{
    public CustomManageModel(IOptions<ProfileManagementPageOptions> options) : base(options)
    {
        this.LocalizationResourceType = typeof(ServerResource);
    }
}
