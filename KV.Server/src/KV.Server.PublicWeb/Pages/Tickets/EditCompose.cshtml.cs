namespace KV.Server.PublicWeb.Pages.Tickets;
using System.ComponentModel.DataAnnotations;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Timing;

[Authorize]
public class EditComposeModel : ServerPageModel
{
    public const string TicketStatusDraftLower = "draft";
    public const string TicketStatusNewLower = "new";
    private readonly IContractsAppService _contractsAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly ITicketFileAppService _ticketFileAppService;

    private readonly ITicketsPublicAppService _ticketsAppService;
    private readonly ITicketSectionAppService _ticketSectionAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;
    private readonly ITicketTypeAppService _ticketTypeAppService;

    public TicketStatusInfoDto FavoriteTicket { get; set; } = new();

    public TicketStatusInfoDto MyTicket { get; set; } = new();

    public List<ServicePackageDto> TicketActiveServicePackagesView { get; set; } = new();

    public List<ServicePackageDto> TicketNotActiveServicePackagesView { get; set; } = new();

    public List<TicketStatusInfoDto> TicketStatusesView { get; set; } = new();

    public List<TicketTypeDto> TicketTypesView { get; set; } = new();

    public EditComposeModel(ITicketsPublicAppService ticketsAppService,
        ITicketStatusAppService ticketStatusAppService,
        ITicketFileAppService ticketFileAppService,
        ITicketTypeAppService ticketTypeAppService,
        ITicketSectionAppService ticketSectionAppService,
        IContractsAppService contractsAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppServic)
    {
        this._ticketsAppService = ticketsAppService;
        this._ticketStatusAppService = ticketStatusAppService;
        this._ticketFileAppService = ticketFileAppService;
        this._ticketTypeAppService = ticketTypeAppService;
        this._ticketSectionAppService = ticketSectionAppService;
        this._contractsAppService = contractsAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppServic;
    }

    [BindProperty] public CreateUpdateTicketDto Ticket { get; set; } = new();

    [BindProperty] public string UserSelectContractId { get; set; } = string.Empty;

    [BindProperty][Required] public string UserSelectTicketTypeId { get; set; } = string.Empty;

    [BindProperty] public long TicketId { get; set; }

    [BindProperty] public string? UserSelectTicketSectionId { get; set; }

    public List<SelectListItem> SelectListTicketType { get; set; } = new();

    public List<SelectListItem> SelectListTicketSection { get; set; } = new();

    public List<SelectListItem> SelectListContract { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(long id)
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

        var ticketTypes = await this._ticketTypeAppService
            .GetTicketTypesAsync();
        var ticketSections = (await this._ticketSectionAppService
                .GetTicketSectionsAsync())
            .OrderBy(x => x.Name);
        var contracts = (await this._contractsAppService
                .GetListContractByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty))
            .OrderBy(x => x.ContractFinishDate);
        var ticket = await this._ticketsAppService.GetTicketDetailsAsync(id);
        this.TicketId = id;
        this.Ticket.Subject = ticket.Subject;
        this.Ticket.Description = ticket.Description;
        this.Ticket.TicketTypeId = ticket.TicketTypeId;
        this.Ticket.TicketStatusId = ticket.TicketStatusId;
        this.Ticket.TicketSectionId = ticket.TicketSectionId;
        this.UserSelectTicketTypeId = ticket.TicketTypeId.ToString();
        this.UserSelectTicketSectionId = ticket.TicketSectionId?.ToString();
        if (ticket.CreatorId != this.CurrentUser?.Id)
        {
            return this.NoContent();
        }

        if (contracts.LastOrDefault()?.ContractFinishDate < this.Clock.Now)
        {
            if (this.CurrentTenant?.Id != null || this.CurrentUser?.Id == null)
            {
                throw new UserFriendlyException("Не найдены контракты.");
                // TODO: display modal, write on mail
            }
        }

        if (contracts != null && contracts.Count() == 1)
        {
            this.UserSelectContractId = contracts.FirstOrDefault()?.Id.ToString() ?? Guid.Empty.ToString("D");
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
            new() { Value = "", Text = "          " }
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
            new() { Value = this.CurrentTenant?.Id == null ? null : "", Text = "          " }
        };
        foreach (var contract in contracts?.ToList() ?? new List<ContractDto>())
        {
            var servicePackagesName = string.Empty;
            foreach (var servicePacakge in contract.ServicePackages)
            {
                servicePackagesName += servicePacakge.Name + ", ";
            }

            servicePackagesName = servicePackagesName.TrimEnd(' ').TrimEnd(',');
            if (contract.ContractFinishDate > DateTime.Now)
            {
                var contractName =
                    $"{contract.Name} (   {contract.ContractStartDate:dd.MM.yyyy}    {contract.ContractFinishDate:dd.MM.yyyy}),              ({servicePackagesName})";
                this.SelectListContract.Add(new SelectListItem { Value = contract.Id.ToString(), Text = contractName });
            }
        }

        var customerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser?.Id ?? Guid.Empty);
        this.MyTicket = await this._ticketStatusAppService.GetMyTicketStatusByResponsibleIdAsync(customerUserProfile?.Id ??
            Guid.Empty);
        this.FavoriteTicket = await this._ticketStatusAppService.GetTicketFavoriteInfoAsync();
        return this.Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        this.Ticket.TicketTypeId = Guid.Parse(this.UserSelectTicketTypeId);
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

        await this._ticketsAppService.UpdateTicketDraftAsync(this.TicketId, this.Ticket);
        if (this.HttpContext.Request.Form.Files.Count != 0)
        {
            var formFiles = this.HttpContext.Request.Form.Files;
            foreach (var formFile in formFiles)
            {
                await this._ticketFileAppService.UploadTicketAttachmentAsync(this.TicketId,
                    new RemoteStreamContent(formFile.OpenReadStream(),
                        formFile.FileName, formFile.ContentType));
            }
        }

        return this.Redirect($"details/{this.TicketId}");
    }
}
