namespace Abp.Identity.Web.Pages.Identity.Users;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Identity;

public class CreateModalModel : Volo.Abp.Identity.Web.Pages.Identity.Users.CreateModalModel
{
    private readonly IIdentityUserAppService _identityUserAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    public CreateModalModel(ICustomerUserProfilesAppService customerUserProfilesAppService,
        IIdentityUserAppService identityUserAppService) : base(identityUserAppService)
    {
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._identityUserAppService = identityUserAppService;
    }

    public override async Task<NoContentResult> OnPostAsync()
    {
        this.ValidateModel();

        var input = this.ObjectMapper.Map<UserInfoViewModel, IdentityUserCreateDto>(this.UserInfo);
        input.RoleNames = this.Roles.Where(r => r.IsAssigned).Select(r => r.Name).ToArray();
        input.IsActive = true;
        var identityUser = await this.IdentityUserAppService.CreateAsync(input);
        await this._customerUserProfilesAppService.CreateAsync(new KV.Server.CreateUpdateCustomerUserProfileDto()
        {
            LastName = identityUser.Surname,
            FirstName = identityUser.Name,
            JobPost = input.RoleNames.FirstOrDefault(),
            Country = "Россия",
            IdentityUserId = identityUser.Id,
            Id = identityUser.Id,
            TenantId = identityUser.TenantId,
            CreationTime = identityUser.CreationTime
        });
        return this.NoContent();
    }
}
