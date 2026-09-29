namespace KV.Server.PublicWeb.Pages.Tickets;
using System.ComponentModel.DataAnnotations;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Content;

[Authorize]
public class ComposeModel : ServerPageModel
{
    public const string TicketStatusDraftLower = "draft";
    private readonly IConfiguration _config;
    private readonly IContractsAppService _contractsAppService;
    private readonly IContractsPublicAppService _contractsPublicAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly ITicketFileAppService _ticketFileAppService;

    private readonly ITicketsPublicAppService _ticketsAppService;
    private readonly ITicketSectionAppService _ticketSectionAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;
    private readonly ITicketTypeAppService _ticketTypeAppService;

    public TicketStatusInfoDto FavoriteTicket { get; set; } = new TicketStatusInfoDto();

    public TicketStatusInfoDto MyTicket { get; set; } = new TicketStatusInfoDto();

    public List<ServicePackageDto> TicketActiveServicePackagesView { get; set; } = new List<ServicePackageDto>();

    public List<ServicePackageDto> TicketNotActiveServicePackagesView { get; set; } = new();

    public List<TicketStatusInfoDto> TicketStatusesView { get; set; } = new();

    public List<TicketTypeDto> TicketTypesView { get; set; } = new();

    public ComposeModel(ITicketsPublicAppService ticketsAppService,
        ITicketStatusAppService ticketStatusAppService,
        ITicketFileAppService ticketFileAppService,
        ITicketTypeAppService ticketTypeAppService,
        ITicketSectionAppService ticketSectionAppService,
        IContractsAppService contractsAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppServic,
        IConfiguration config,
        IContractsPublicAppService contractsPublicAppService)
    {
        this._ticketsAppService = ticketsAppService;
        this._ticketStatusAppService = ticketStatusAppService;
        this._ticketFileAppService = ticketFileAppService;
        this._ticketTypeAppService = ticketTypeAppService;
        this._ticketSectionAppService = ticketSectionAppService;
        this._contractsAppService = contractsAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppServic;
        this._config = config;
        _contractsPublicAppService = contractsPublicAppService;
    }

    [BindProperty] public CreateUpdateTicketDto Ticket { get; set; } = new();

    [BindProperty] public string? UserSelectContractId { get; set; }

    [BindProperty][Required] public string? UserSelectTicketTypeId { get; set; }

    [BindProperty] public string? UserSelectTicketSectionId { get; set; }

    [BindProperty] public string? TicketStatusArchiveId { get; set; }

    [BindProperty] public string? TicketStatusNewId { get; set; }

    public List<SelectListItem> SelectListTicketType { get; set; } = new();

    public List<SelectListItem> SelectListTicketSection { get; set; } = new();

    public List<SelectListItem> SelectListContract { get; set; } = new();

    public bool ClientHasContract { get; set; } = true;

    public async Task<IActionResult> OnGetAsync()
    {
        this.TicketStatusesView = await this._ticketStatusAppService.GetTicketStatusByCreatorIdAndTenantAsync(this.CurrentUser?.Id);

        this.TicketActiveServicePackagesView =
            await this._contractsAppService.GetListActiveServicePackagesByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty);
        this.TicketNotActiveServicePackagesView =
            await this._contractsAppService.GetListNotActiveServicePackagesByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty);
        this.Ticket = new CreateUpdateTicketDto
        {
            CreatorId = this.CurrentUser?.Id ?? Guid.Empty
        };
        this.TicketStatusNewId = this._config["Ticket:TicketStatusId"];
        Guid.TryParse(this.TicketStatusNewId, out var ticketStatusId);
        this.Ticket.TicketStatusId = ticketStatusId;
        this.TicketStatusArchiveId = (this.TicketStatusesView.FirstOrDefault(
            x => x.Name.ToLowerInvariant().Contains(TicketStatusDraftLower))?.Id ?? this.Ticket.TicketStatusId).ToString();
        var ticketTypes = await this._ticketTypeAppService
            .GetTicketTypesAsync();
        var ticketSections = (await this._ticketSectionAppService
            .GetTicketSectionsAsync())
            .OrderBy(x => x.Name);
        var contract = await this._contractsPublicAppService
            .GetFirstActiveContractAsync();

        this.UserSelectContractId = contract?.Id.ToString("D") ?? Guid.Empty.ToString("D");

        if (contract != null)
        {
            this.Ticket.ContractId = this.UserSelectContractId.To<Guid>();
        }
        this.SelectListTicketType = new List<SelectListItem>();
        foreach (var ticketType in ticketTypes)
        {
            this.SelectListTicketType.Add(new SelectListItem
            {
                Text = ticketType.Name,
                Value = ticketType.Id.ToString()
            });
        }

        this.SelectListTicketSection = new List<SelectListItem>
        {
            new() { Value = "", Text = "Не выбрано" }
        };
        foreach (var ticketSection in ticketSections)
        {
            this.SelectListTicketSection.Add(new SelectListItem
            {
                Text = ticketSection.Name,
                Value = ticketSection.Id.ToString()
            });
        }

        this.SelectListContract = new List<SelectListItem>
        {
            new() { Value = this.CurrentTenant?.Id == null ? null : "", Text = "Не выбрано" }
        };
        var contracts = await _contractsPublicAppService.GetListActiveContractByTenantIdAsync(this.CurrentTenant?.Id);
        foreach (var item in contracts)
        {
            var servicePackagesName = string.Empty;
            foreach (var servicePacakge in item.ServicePackages)
            {
                servicePackagesName += servicePacakge.Name + ", ";
            }

            servicePackagesName = servicePackagesName.TrimEnd(' ').TrimEnd(',');
            if (item.ContractFinishDate > DateTime.Now)
            {
                var contractName =
                    $"{item.Name} (от {item.ContractStartDate:dd.MM.yyyy} до {item.ContractFinishDate:dd.MM.yyyy}), пакеты услуг ({servicePackagesName})";
                this.SelectListContract.Add(new SelectListItem { Value = item.Id.ToString(), Text = contractName });
            }
        }

        var currentCustomerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser?.Id ?? Guid.Empty);
        this.MyTicket = await this._ticketStatusAppService.GetMyTicketStatusByResponsibleIdAsync(currentCustomerUserProfile?.Id ??
            Guid.Empty);
        if (this.Ticket.ContractId == Guid.Empty)
        {
            if (this.CurrentTenant?.Id != null || this.CurrentUser?.Id == null)
            {
                this.ClientHasContract = false;
            }
        }
        else
        {
            this.FavoriteTicket = await this._ticketStatusAppService.GetTicketFavoriteInfoAsync();
        }
        return this.Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Guid.TryParse(this.UserSelectTicketTypeId, out var userSelectTicketTypeId);
        this.Ticket.TicketTypeId = userSelectTicketTypeId;
        if (!string.IsNullOrWhiteSpace(this.UserSelectTicketSectionId))
        {
            this.Ticket.TicketSectionId = Guid.Parse(this.UserSelectTicketSectionId);
        }

        if (!string.IsNullOrWhiteSpace(this.UserSelectContractId))
        {
            if (Guid.TryParse(this.UserSelectContractId, out var contractId))
            {
                this.Ticket.ContractId = contractId;
            }
        }

        var createdTicket = await this._ticketsAppService.CreateTicketAsync(this.Ticket);
        if (this.HttpContext.Request.Form.Files.Count != 0)
        {
            var formFiles = this.HttpContext.Request.Form.Files;
            foreach (var formFile in formFiles)
            {
                await this._ticketFileAppService.UploadTicketAttachmentAsync(createdTicket.Id,
                    new RemoteStreamContent(formFile.OpenReadStream(),
                        formFile.FileName, formFile.ContentType));
            }
        }

        var ticketStatus = await this._ticketStatusAppService.GetAsync(this.Ticket.TicketStatusId);
        if (ticketStatus.IsPublic == false)
        {
            return this.Redirect($"/tickets/editcompose?id={createdTicket.Id}");
        }

        return this.Redirect($"details/{createdTicket.Id}");
    }
}
