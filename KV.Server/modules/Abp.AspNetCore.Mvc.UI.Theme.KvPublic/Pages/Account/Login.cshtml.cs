namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Pages.Account;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using KV.Server.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Volo.Abp.Account.Settings;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

public class CustomLoginModel : LoginModel
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _identityRepository;
    public CustomLoginModel(IAuthenticationSchemeProvider schemeProvider,
        IOptions<AbpAccountOptions> accountOptions,
        IOptions<IdentityOptions> identityOptions,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> identityRepository,
        IDataFilter dataFilter,
        IdentityDynamicClaimsPrincipalContributorCache identityDynamicClaimsPrincipalContributorCache)
            : base(schemeProvider, accountOptions, identityOptions, identityDynamicClaimsPrincipalContributorCache)
    {
        this.LocalizationResourceType = typeof(ServerResource);
        this._identityRepository = identityRepository;
        this._dataFilter = dataFilter;
    }

    public override async Task<IActionResult> OnPostAsync(string action)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            await this.CheckLocalLoginAsync();

            this.ValidateModel();

            this.ExternalProviders = await this.GetExternalProviders();

            this.EnableLocalLogin = await this.SettingProvider.IsTrueAsync(AccountSettingNames.EnableLocalLogin);

            await this.ReplaceEmailToUsernameOfInputIfNeeds();

            await this.IdentityOptions.SetAsync();

            var result = await this.SignInManager.PasswordSignInAsync(
                this.LoginInput.UserNameOrEmailAddress,
                this.LoginInput.Password,
                this.LoginInput.RememberMe,
                true
            );

            await this.IdentitySecurityLogManager.SaveAsync(new IdentitySecurityLogContext()
            {
                Identity = IdentitySecurityLogIdentityConsts.Identity,
                Action = result.ToIdentitySecurityLogAction(),
                UserName = this.LoginInput.UserNameOrEmailAddress
            });

            if (result.RequiresTwoFactor)
            {
                return await this.TwoFactorLoginResultAsync();
            }

            if (result.IsLockedOut)
            {
                this.Alerts.Warning(this.L["UserLockedOutMessage"]);
                return this.Page();
            }

            if (result.IsNotAllowed)
            {
                this.Alerts.Warning(this.L["LoginIsNotAllowed"]);
                return this.Page();
            }

            if (!result.Succeeded)
            {
                this.Alerts.Danger(this.L["InvalidUserNameOrPassword"]);
                return this.Page();
            }

            //TODO: Find a way of getting user's id from the logged in user and do not query it again like that!
            var user = await this.UserManager.FindByNameAsync(this.LoginInput.UserNameOrEmailAddress) ??
                        await this.UserManager.FindByEmailAsync(this.LoginInput.UserNameOrEmailAddress);
            Debug.Assert(user != null, nameof(user) + " != null");

            return this.RedirectSafely(this.ReturnUrl, this.ReturnUrlHash);
        }
    }
}
