namespace KV.Server.Web.Pages.Users;
using System;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class IndexModel : ServerPageModel
{
    private readonly ITableAppService _tableAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    public IndexModel(ITableAppService tableAppService, ICustomerUserProfilesAppService customerUserProfilesAppService)
    {
        this._tableAppService = tableAppService;
        _customerUserProfilesAppService = customerUserProfilesAppService;
    }

    [BindProperty(SupportsGet = true)]
    public UserSearchViewModel UserSearchViewModelData { get; set; }
    [BindProperty]
    public Guid? CurrentUserId { get; set; }
    public async void OnGet()
    {
        this.UserSearchViewModelData = new UserSearchViewModel();
    }

    public async Task<IActionResult> OnGetExcelTableAsync()
    {

        var input = this.ObjectMapper.Map<UserSearchViewModel, GetUserListRequestDto>(this.UserSearchViewModelData);
        input.MaxResultCount = 100;
        var remoteStream = await this._tableAppService.GenerateUserTableAsync(input);
        return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
    }

    public class UserSearchViewModel
    {
        public string FullName { get; set; }
        public string JobPost { get; set; }
        public string Direction { get; set; }
        public string TenantShortName { get; set; }
        [HiddenInput] public string ResponsibleManagerId { get; set; }
        public DateTime? ActualizationTime { get; set; }
    }
}
