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
using Volo.Abp.Identity;

[Authorize]
public class DetailsModel : ServerPageModel
{
    public const string ResponsibleManagerRoleName = "Ответственный менеджер";
    private readonly ICrudContractStatusService _contractsStatusAppService;
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly IEmployeeProfileAppService _employeeProfileAppService;
    private readonly IIdentityUserAppService _identityUserAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;
    private readonly ITableAppService _tableAppService;
    private readonly IRegionsAppService _regionsAppService;

    public DetailsModel(ICrudCustomerAppService crudCustomerAppService,
        ICrudContractStatusService contractsStatusAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        IEmployeeProfileAppService employeeProfileAppService,
        IIdentityUserAppService identityUserAppService,
        ITicketStatusAppService ticketStatusAppService,
        ITableAppService tableAppService,
        IRegionsAppService regionsAppService)
    {
        this._crudCustomerAppService = crudCustomerAppService;
        this._contractsStatusAppService = contractsStatusAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        _employeeProfileAppService = employeeProfileAppService;
        this._identityUserAppService = identityUserAppService;
        this._ticketStatusAppService = ticketStatusAppService;
        this._tableAppService = tableAppService;
        _regionsAppService = regionsAppService;
    }

    [BindProperty] public CustomerViewModel Customer { get; set; }

    [BindProperty] public SearchViewModel Search { get; set; }

    [BindProperty] public UpdateCustomerViewModel UpdateCustomer { get; set; }

    [BindProperty(SupportsGet = true)]
    public SearchStatisticsViewModel SearchStatistics { get; set; }

    public List<CustomerServicePackageDto> ContractServicePackages { get; set; } = new();

    public List<SelectListItem> TicketStatuses { get; set; }
    public List<CustomerStatisticsDto> Statistics { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        this.SearchStatistics = new()
        {
            TenantId = id.ToString()
        };
        this.Search = new SearchViewModel();
        this.Customer = new CustomerViewModel
        {
            Id = id
        };
        var dto = await this._crudCustomerAppService.GetAsync(this.Customer.Id);
        this.Customer = this.ObjectMapper.Map<CustomerDto, CustomerViewModel>(dto);
        Customer.ResponsibleFullName = dto.ResponsibleFullName;

        if (dto.RegionId is not null)
        {
            Customer.Region = (await _regionsAppService.GetListAsync()).Items?.FirstOrDefault(x => x.Id == dto.RegionId)?.Name ?? string.Empty;
        }

        this.TicketStatuses = (await this._ticketStatusAppService.GetListStatusesAsync())
            .RenderToSelectList(x => x.DisplayNameMany, x => x.Id.ToString())
            .ToList();
        this.TicketStatuses.AddFirst(new SelectListItem { Text = string.Empty, Value = string.Empty });

        this.UpdateCustomer = new UpdateCustomerViewModel
        {
            LastManagerId = dto.ResponsibleManagerId,
            NewManagerId = dto.ResponsibleManagerId,
            Id = id
        };

        this.ContractServicePackages = dto.ServicePackagesData is not null ? JsonConvert.DeserializeObject<List<CustomerServicePackageDto>>(dto.ServicePackagesData) : new();

        this.Statistics = await this._crudCustomerAppService.GetStatisticsAsync(new GetCustomerStatisticsListRequestDto
        {
            TenantId = id
        });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (this.UpdateCustomer.LastManagerId != null)
        {
            var countResponsibleManager =
                await this._crudCustomerAppService.GetCountResponsibleManagerByCustomerUserProfileIdAsync(
                    this.UpdateCustomer.LastManagerId ?? Guid.Empty);
            if (countResponsibleManager <= 1)
            {
                var lastManager =
                    await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(
                        this.UpdateCustomer.LastManagerId ?? Guid.Empty);
                var lastManagerRoles = await this._identityUserAppService.GetRolesAsync(lastManager.IdentityUserId);
                var lastManagerNameRoles = lastManagerRoles.Items.Select(x => x.Name).ToList();
                if (lastManagerNameRoles.Remove(ResponsibleManagerRoleName))
                {
                    await this._identityUserAppService.UpdateRolesAsync(lastManager.IdentityUserId,
                        new IdentityUserUpdateRolesDto
                        {
                            RoleNames = lastManagerNameRoles.ToArray()
                        });
                }
            }
        }

        var newManager =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.UpdateCustomer.NewManagerId ??
                Guid.Empty);
        var roles = await this._identityUserAppService.GetRolesAsync(newManager.IdentityUserId);
        var nameRoles = roles.Items.Select(x => x.Name).ToList();
        if (!nameRoles.Contains(ResponsibleManagerRoleName))
        {
            nameRoles.Add(ResponsibleManagerRoleName);
            await this._identityUserAppService.UpdateRolesAsync(newManager.IdentityUserId, new IdentityUserUpdateRolesDto
            {
                RoleNames = nameRoles.ToArray()
            });
        }

        await this._crudCustomerAppService.UpdateResponsibleManagerByTenantIdAsync(this.UpdateCustomer.NewManagerId,
            this.UpdateCustomer.Id);
        return this.NoContent();
    }

    public async Task<IActionResult> OnGetExcelTableStatisticsAsync(Guid id)
    {
        var input = this.ObjectMapper.Map<SearchStatisticsViewModel, GetCustomerStatisticsListRequestDto>(this.SearchStatistics);
        input.TenantId = id;
        var remoteStream = await this._tableAppService.GenerateStatisticsTableAsync(input);
        return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
    }

    public async Task<IActionResult> OnGetExcelTableCustomerAsync(Guid id)
    {
        var remoteStream = await _tableAppService.GenerateCustomerUserProfileTableAsync(new() { CustomerId = id });
        return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
    }

    public async Task<IActionResult> OnGetExcelTableContractAsync(Guid id)
    {
        var remoteStream = await _tableAppService.GenerateContractTableAsync(new(){ TenantId = id});
        return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
    }

    public class UpdateCustomerViewModel
    {
        [HiddenInput] public Guid Id { get; set; }

        public Guid? NewManagerId { get; set; }

        [HiddenInput] public Guid? LastManagerId { get; set; }
    }

    public class CustomerViewModel
    {
        [HiddenInput] public Guid Id { get; set; }

        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string Region { get; set; }
        public string Address { get; set; }
        public string SiteUrl { get; set; }
        public string ContractEmail { get; set; }
        public string ContractPhoneNumber { get; set; }
        public string Description { get; set; }
        public string INNNumber { get; set; }
        public string KPPNumber { get; set; }
        public string CommentNotes { get; set; }
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }
        public string ContractOwner { get; set; }
        public string ResponsibleFullName { get; set; }
        public string OrganizationName { get; set; }

        public DateTime CreationTime { get; set; }
    }

    public class SearchStatisticsViewModel
    {
        public DateTime StartPeriod { get; set; } = DateTime.Now.AddMonths(-24);
        public DateTime EndPeriod { get; set; } = DateTime.Now;
        public string TenantId { get; set; }
    }

    public class SearchViewModel
    {
        [DataType(DataType.Text)] public string FirstName { get; set; }

        [DataType(DataType.Text)] public string LastName { get; set; }

        [DataType(DataType.Text)] public string MiddleName { get; set; }

        [DataType(DataType.DateTime)] public DateTime? DateOfBirthDay { get; set; } = DateTime.Now;

        [DataType(DataType.Text)] public string JobPost { get; set; }

        [DataType(DataType.Text)] public string ContractName { get; set; }

        [DataType(DataType.DateTime)] public DateTime? ContractStartDate { get; set; }

        [DataType(DataType.DateTime)] public DateTime? ContractFinishDate { get; set; }

        [DataType(DataType.Text)] public string ContractStatusId { get; set; }

        [DataType(DataType.Text)] public string TicketSubject { get; set; }

        [DataType(DataType.Text)] public DateTime? TicketCreationTime { get; set; }

        [DataType(DataType.Text)] public DateTime? TicketDueDate { get; set; }

        [DataType(DataType.Text)] public string TicketCreatorFullName { get; set; }

        [DataType(DataType.Text)] public string TicketResposibleFullName { get; set; }

        [DataType(DataType.Text)] public string TicketStatusId { get; set; }
    }
}
