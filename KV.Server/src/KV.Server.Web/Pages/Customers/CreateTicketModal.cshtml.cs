namespace KV.Server.Web.Pages.Customers;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class CreateTicketModalModel : ServerPageModel
{
    private readonly ITicketsAppService _ticketsAppService;
    private readonly IContractsAppService _crudContractService;
    private readonly ICrudContractStatusService _crudContractStatusService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;
    private readonly ITicketTypeAppService _ticketTypeAppService;
    public CreateTicketModalModel(ITicketsAppService ticketsAppService, ICrudContractStatusService crudContractStatusService, ITicketTypeAppService ticketTypeAppService, ITicketStatusAppService ticketStatusAppService, IContractsAppService crudContractService, ICustomerUserProfilesAppService customerUserProfilesAppService)
    {
        this._ticketsAppService = ticketsAppService;
        this._crudContractStatusService = crudContractStatusService;
        this._ticketTypeAppService = ticketTypeAppService;
        this._ticketStatusAppService = ticketStatusAppService;
        _crudContractService = crudContractService;
        _customerUserProfilesAppService = customerUserProfilesAppService;
    }

    public List<SelectListItem> Statuses { get; set; }
    public List<SelectListItem> TicketTypes { get; set; }
    public List<SelectListItem> Contracts { get; set; }
    [BindProperty]
    public CreateTicketVM CreateTicketVM { get; set; }
    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid Id { get; set; }
    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid? TenantId { get; set; }
    public async Task OnGetAsync(Guid id)
    {
        this.Statuses = new List<SelectListItem>();
        this.TicketTypes = new List<SelectListItem>();
        Contracts = new List<SelectListItem>();
        this.Id = id;
        this.CreateTicketVM = new CreateTicketVM();
        var customer = await _customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(id);
        TenantId = customer.TenantId;
        var contracts = await _crudContractService.GetListActiveContractByTenantIdAsync(customer.TenantId);
        var ticketTypes = await this._ticketTypeAppService.GetTicketTypesAsync();
        foreach (var item in contracts)
        {
            Contracts.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
        }
        foreach (var item in ticketTypes)
        {
            this.TicketTypes.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
        }
        var statuses = await this._ticketStatusAppService.GetListStatusesAsync();
        foreach (var item in statuses)
        {
            this.Statuses.Add(new SelectListItem { Text = item.DisplayName, Value = item.Id.ToString() });
        }
    }

    public async Task OnPostAsync()
    {
        this.CreateTicketVM.CreatorId = this.Id;
        var dto = this.ObjectMapper.Map<CreateTicketVM, CreateUpdateTicketDto>(this.CreateTicketVM);

        dto.TenantId = TenantId;

        await this._ticketsAppService.CreateTicketAsync(dto);
    }
}
public class CreateTicketVM
{
    [Required] public string Subject { get; set; }
    [Required] public string Description { get; set; }
    public Guid TicketTypeId { get; set; }
    public Guid TicketStatusId { get; set; }
    [HiddenInput]
    public Guid? TicketSectionId { get; set; }
    [HiddenInput]
    public Guid CreatorId { get; set; }
    [HiddenInput]
    public Guid ContractId { get; set; }
    [HiddenInput] public DateTime? ReadByClientDate { get; set; }
    [HiddenInput] public DateTime? ReadBySpecialistDate { get; set; }
}
