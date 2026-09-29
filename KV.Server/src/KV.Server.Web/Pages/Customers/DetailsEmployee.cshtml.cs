namespace KV.Server.Web.Pages.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

[Authorize]
public class DetailsEmployeeModel : ServerPageModel
{
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly IContractsAppService _contractsAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly IUsersAppService _usersAppService;

    public DetailsEmployeeModel(ICustomerUserProfilesAppService customerUserProfilesAppService,
        ITicketStatusAppService ticketStatusAppService,
        IContractsAppService contractsAppService,
        ICrudCustomerAppService crudCustomerAppService,
        IUsersAppService usersAppService)
    {
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._ticketStatusAppService = ticketStatusAppService;
        _contractsAppService = contractsAppService;
        _crudCustomerAppService = crudCustomerAppService;
        _usersAppService = usersAppService;
    }

    [BindProperty] public UpdateEmployeeViewModel EditEmployee { get; set; }

    [BindProperty] public SearchEmployeeViewModel Search { get; set; }

    public List<SelectListItem> TicketStatuses { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        var employee = await this._customerUserProfilesAppService.GetAsync(id);

        this.EditEmployee = this.ObjectMapper.Map<CustomerUserProfileDto, UpdateEmployeeViewModel>(employee);
        var contract = await _contractsAppService.GetFirstActiveContractAsync((Guid)employee.TenantId);
        var client = await _crudCustomerAppService.GetAsync((Guid)employee.TenantId);
        EditEmployee.OrganizationName = client.DisplayName;
        if (client != null)
        {
            EditEmployee.INN = client.INNNumber;
            EditEmployee.KPP = client.KPPNumber;
            EditEmployee.Responsible = client.ResponsibleManager ?? "Îòñóòñòâóåò";
        }
        EditEmployee.Site = employee.Site;

        if (contract != null)
        {
            EditEmployee.ServicePackagesData = client.ServicePackagesData is not null ? JsonConvert.DeserializeObject<List<CustomerServicePackageDto>>(client.ServicePackagesData) : new();
        }

        this.Search = new SearchEmployeeViewModel();

        this.TicketStatuses = (await this._ticketStatusAppService.GetListStatusesAsync())
            .RenderToSelectList(x => x.DisplayNameMany, x => x.Id.ToString())
            .ToList();
        this.TicketStatuses.AddFirst(new SelectListItem { Text = string.Empty, Value = string.Empty });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var customerDto = this.ObjectMapper.Map<UpdateEmployeeViewModel, CreateUpdateCustomerUserProfileDto>(this.EditEmployee);
        await this._customerUserProfilesAppService.UpdateAsync(customerDto);
        return this.NoContent();
    }
}

public class UpdateEmployeeViewModel
{
    [Required] public string LastName { get; set; }

    [Required] public string FirstName { get; set; }

    public string MiddleName { get; set; }

    [DataType(DataType.Text)] public DateTime? DateOfBirthDay { get; set; }

    [Required] public string JobPost { get; set; }

    [DataType(DataType.PhoneNumber)] public string PhoneNumber { get; set; }

    public string Country { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string Note { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public string ContactUserFIO { get; set; }
    public string OrganizationName { get; set; }
    public string INN { get; set; }
    public string KPP { get; set; }
    public string Site { get; set; }
    public List<CustomerServicePackageDto> ServicePackagesData { get; set; } = new();
    public string Responsible { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }

    [HiddenInput] public Guid Id { get; set; }

    [HiddenInput] public Guid TenantId { get; set; }

    [HiddenInput] public Guid IdentityUserId { get; set; }

    [HiddenInput] public Guid? UserAvatarFileId { get; set; }
}

public class SearchEmployeeViewModel
{
    [DataType(DataType.Text)] public string TicketSubject { get; set; }

    [DataType(DataType.Text)] public DateTime? TicketCreationTime { get; set; }

    [DataType(DataType.Text)] public DateTime? TicketDueDate { get; set; }

    [DataType(DataType.Text)] public string TicketResposibleFullName { get; set; }

    [DataType(DataType.Text)] public string TicketStatusId { get; set; }

    [DataType(DataType.Text)] public string TicketCustomerShortName { get; set; }
}
