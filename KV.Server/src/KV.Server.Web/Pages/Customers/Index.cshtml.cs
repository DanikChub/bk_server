namespace Server.Web.Pages.Customers;
using System;
using System.Linq;
using System.Threading.Tasks;
using KV.Server;
using KV.Server.Interfaces;
using KV.Server.Web.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class IndexModel : ServerPageModel
{
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly ITableAppService _tableAppService;

    public IndexModel(ICustomerUserProfilesAppService customerUserProfilesAppService,
        ITableAppService tableAppService)
    {
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._tableAppService = tableAppService;
    }

    [BindProperty(SupportsGet = true)]
    public CustomerSearchViewModel Search { get; set; }
    public async Task OnGetAsync()
    {
       
    }

    public async Task<IActionResult> OnGetExcelTableAsync()
    {
        var input = this.ObjectMapper.Map<CustomerSearchViewModel, GetCustomerListRequestDto>(this.Search);
        input.MaxResultCount = 100;
        var remoteStream = await this._tableAppService.GenerateCustomerTableAsync(input);
        return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
    }

    public class CustomerSearchViewModel
    {
        public string SearchStr { get; set; }

        public string Address { get; set; }
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string ResponsibleManager { get; set; }
        [HiddenInput] public string ResponsibleManagerId { get; set; }
        [HiddenInput] public Guid? ManagerId { get; set; }
    }
}
