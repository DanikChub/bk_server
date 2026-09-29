namespace Abp.Identity.Web.Pages.Identity.Users;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Identity;

public class EditModalModel : Volo.Abp.Identity.Web.Pages.Identity.Users.EditModalModel
{
    private readonly IIdentityUserAppService _identityUserAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    public EditModalModel(IIdentityUserAppService identityUserAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService) : base(identityUserAppService)
    {
        this._identityUserAppService = identityUserAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
    }

    public override async Task<IActionResult> OnPostAsync()
    {
        this.ValidateModel();

        var input = this.ObjectMapper.Map<UserInfoViewModel, IdentityUserUpdateDto>(this.UserInfo);
        input.RoleNames = this.Roles.Where(r => r.IsAssigned).Select(r => r.Name).ToArray();
        await this.IdentityUserAppService.UpdateAsync(this.UserInfo.Id, input);

        return this.NoContent();
    }
}
