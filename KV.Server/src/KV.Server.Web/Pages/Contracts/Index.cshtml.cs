namespace KV.Server.Web.Pages.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class IndexModel : ServerPageModel
{
    private readonly ICrudContractStatusService _crudContractStatusService;
    private readonly ICrudServicePackageService _crudServicePackageService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly ITableAppService _tableAppService;

    public IndexModel(ICrudContractStatusService crudContractStatusService,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        ICrudServicePackageService crudServicePackageService,
        ITableAppService tableAppService)
    {
        this._crudContractStatusService = crudContractStatusService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._crudServicePackageService = crudServicePackageService;
        this._tableAppService = tableAppService;
    }

    [BindProperty(SupportsGet = true)]
    public ContractSearchViewModel Contract { get; set; }

    public List<SelectListItem> ContractStatuses { get; set; }

    public List<SelectListItem> ServicePackages { get; set; }

    public async Task OnGetAsync()
    {
        this.Contract = new ContractSearchViewModel();
        this.ContractStatuses = new List<SelectListItem>
        {
            new SelectListItem { Text = string.Empty, Value = string.Empty }
        };
        var contractStasuses = await this._crudContractStatusService.GetContractStatusesListAsync();
        foreach (var contractStatus in contractStasuses)
        {
            this.ContractStatuses.Add(new SelectListItem
            { Text = contractStatus.Title, Value = contractStatus.Id.ToString() });
        }

        this.ServicePackages = new List<SelectListItem>
        {
            new SelectListItem { Text = "Все", Value = string.Empty }
        };
        var servicePakcages = await this._crudServicePackageService.ToListAsync();
        foreach (var servicePakcage in servicePakcages)
        {
            this.ServicePackages.Add(new SelectListItem
            { Text = servicePakcage.Name, Value = servicePakcage.Id.ToString() });
        }

        if (this.CurrentUser != null)
        {
            var customerUserProfile =
                await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser.Id ?? Guid.Empty);
            if (this.CurrentUser.IsInRole("Менеджер Отдела Продаж"))
            {
                this.Contract.ManagerId = customerUserProfile.Id;
            }
        }
    }

    public async Task<IActionResult> OnGetExcelTableAsync()
    {
        var input = this.ObjectMapper.Map<ContractSearchViewModel, GetContractListRequestDto>(this.Contract);
        var remoteStream = await this._tableAppService.GenerateContractTableAsync(input);
        return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
    }

    public class ContractSearchViewModel
    {
        [DataType(DataType.Text)] public string Name { get; set; }

        [DataType(DataType.DateTime)] public DateTime? ContractStartDate { get; set; }

        [DataType(DataType.DateTime)] public DateTime? ContractFinishDate { get; set; }

        [DataType(DataType.Text)] public string StatusId { get; set; }

        [DataType(DataType.Text)] public string TenantProfileShortName { get; set; }

        public string ServicePackageId { get; set; }

        [HiddenInput] public Guid? ManagerId { get; set; }
    }
}
