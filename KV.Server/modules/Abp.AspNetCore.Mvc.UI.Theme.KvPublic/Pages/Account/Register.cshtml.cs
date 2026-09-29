namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Pages.Account;

using KV.Server.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Identity;

public class CustomRegisterModel : RegisterModel
{
    public CustomRegisterModel(IAccountAppService accountAppService,
        IAuthenticationSchemeProvider schemeProvider,
        IOptions<AbpAccountOptions> accountOptions,
        IdentityDynamicClaimsPrincipalContributorCache identityDynamicClaimsPrincipalContributorCache)
        : base(accountAppService, schemeProvider, accountOptions, identityDynamicClaimsPrincipalContributorCache)
    {
        this.LocalizationResourceType = typeof(ServerResource);
    }
}
