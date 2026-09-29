namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Pages.Account.Components.ProfileManagementGroup.PersonalInfo;
using Volo.Abp.Account;
using Volo.Abp.Account.Web.Pages.Account.Components.ProfileManagementGroup.PersonalInfo;

public class CustomAccountProfilePersonalInfoManagementGroupViewComponent : AccountProfilePersonalInfoManagementGroupViewComponent
{
    public CustomAccountProfilePersonalInfoManagementGroupViewComponent(
        IProfileAppService profileAppService) : base(profileAppService)
    {

    }
}
