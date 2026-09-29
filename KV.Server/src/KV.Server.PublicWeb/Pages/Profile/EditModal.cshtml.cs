namespace KV.Server.PublicWeb.Pages.Profile;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

public class EditModalModel : ServerPageModel
{
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;

    public EditModalModel(ICustomerUserProfilesAppService customerUserProfilesAppService)
    {
        this._customerUserProfilesAppService = customerUserProfilesAppService;
    }

    [BindProperty] public CustomerUserProfileDto CustomerUserProfile { get; set; } = new();

    public async Task OnGetAsync() => this.CustomerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser?.Id ?? Guid.Empty);

    public async Task OnPostAsync() => await this._customerUserProfilesAppService.UpdateAsync(new CreateUpdateCustomerUserProfileDto
    {
        Id = this.CustomerUserProfile.Id,
        FirstName = this.CustomerUserProfile.FirstName,
        LastName = this.CustomerUserProfile.LastName,
        MiddleName = this.CustomerUserProfile.MiddleName,
        DateOfBirthDay = this.CustomerUserProfile.DateOfBirthDay,
        JobPost = this.CustomerUserProfile.JobPost,
        PhoneNumber = this.CustomerUserProfile.PhoneNumber
    });
}
