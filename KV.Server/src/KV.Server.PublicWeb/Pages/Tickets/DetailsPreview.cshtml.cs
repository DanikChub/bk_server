namespace KV.Server.PublicWeb.Pages.Tickets;
using KV.Server.Dtos.File;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Timing;

[Authorize]
public class DetailsPreviewModel : ServerPageModel
{
    public const string TicketStatusNewLower = "new";
    public const string TicketStatusCloseLower = "answered";
    private readonly IClock _clock;
    private readonly IContractsAppService _contractsAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly ITicketFileAppService _ticketFileAppService;
    private readonly ITicketHistoryAppService _ticketHistoryAppService;
    private readonly ITicketsPublicAppService _ticketsPublicAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;

    public TicketStatusInfoDto MyTicket { get; set; } = new();

    public List<ServicePackageDto> TicketActiveServicePackagesView { get; set; } = new();

    public List<ServicePackageDto> TicketNotActiveServicePackagesView { get; set; } = new();

    public List<TicketStatusInfoDto> TicketStatusesView { get; set; } = new();

    public DetailsPreviewModel(ITicketsPublicAppService ticketsPublicAppService,
        ITicketHistoryAppService ticketHistoryAppService,
        ITicketFileAppService ticketFileAppService,
        ITicketStatusAppService ticketStatusAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        IContractsAppService contractsAppService,
        IClock clock)
    {
        this._ticketsPublicAppService = ticketsPublicAppService;
        this._ticketHistoryAppService = ticketHistoryAppService;
        this._ticketFileAppService = ticketFileAppService;
        this._ticketStatusAppService = ticketStatusAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._contractsAppService = contractsAppService;
        this._clock = clock;
    }

    [BindProperty(Name = "skipPage", SupportsGet = true)]
    public int CurrentPage { get; set; }

    [BindProperty] public Guid? TicketCreatorId { get; set; }

    public TicketDetailsDto Ticket { get; set; } = new();

    [BindProperty] public List<UploadedFileDto> UploadedFilesTicket { get; set; } = new();

    [BindProperty] public CreateTicketHistoryDto CreateTicketHistory { get; set; } = new();

    [BindProperty] public TicketPreviewHistoryViewModel TicketHistory { get; set; } = new();

    public async Task OnGetAsync(long id)
    {
        this.TicketStatusesView = await this._ticketStatusAppService.GetTicketStatusByCreatorIdAndTenantAsync(this.CurrentUser?.Id);
        this.TicketActiveServicePackagesView =
            await this._contractsAppService.GetListActiveServicePackagesByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty);
        this.TicketNotActiveServicePackagesView =
            await this._contractsAppService.GetListNotActiveServicePackagesByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty);
        this.Ticket = await this._ticketsPublicAppService.GetTicketDetailsAsync(id);
        this.TicketCreatorId = this.Ticket?.CreatorId;
        this.UploadedFilesTicket = await this._ticketFileAppService.GetTicketAttachmentsListAsync(id);
        var currentCustomerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser?.Id ?? Guid.Empty);
        var specialist = await this._ticketsPublicAppService.GetSpecialistByTicketIdAsync(id);
        var isClient = this.CurrentTenant?.Id == this.Ticket?.TenantId || this.TicketCreatorId == this.CurrentUser?.Id;
        var isSpecialist = currentCustomerUserProfile?.Id == specialist?.Id;

        var ticketHistoryCount = (int)await this._ticketHistoryAppService.GetCountTicketHistoryAsync(id);
        var ticketHistory = await this._ticketHistoryAppService
            .GetTicketHistoryListAsync(new GetTicketHistoryListRequestDto
            {
                MaxResultCount = ticketHistoryCount == 0
                    ? LimitedResultRequestDto.MaxMaxResultCount
                    : ticketHistoryCount,
                Sorting = "CreationTime ASC"
            }, id);

        this.TicketHistory.TicketHistoryDtos = ticketHistory.Items.ToList();
        this.TicketHistory.TicketId = this.Ticket?.Id ?? id;
        foreach (var ticketHistoryDto in this.TicketHistory.TicketHistoryDtos)
        {
            ticketHistoryDto.Attachments =
                await this._ticketFileAppService.GetTicketHistoryAttachmentsListAsync(ticketHistoryDto.Id);
            if (ticketHistoryDto.CreatorId == specialist?.IdentityUserId)
            {
                ticketHistoryDto.IsSpecialistMessage = true;
            }

            if (isSpecialist && ticketHistoryDto != null && ticketHistoryDto.ReadBySpecialistDate == null)
            {
                await this._ticketHistoryAppService.MakeReadTicketHistoryByIdAsync(ticketHistoryDto.Id,
                    new UpdateReadTicketHistoryDto
                    {
                        ReadByClientDate = ticketHistoryDto?.ReadByClientDate,
                        ReadBySpecialistDate = ticketHistoryDto?.ReadBySpecialistDate ?? this._clock.Now
                    });
            }
            else if (isClient && ticketHistoryDto != null && ticketHistoryDto.ReadByClientDate == null)
            {
                await this._ticketHistoryAppService.MakeReadTicketHistoryByIdAsync(ticketHistoryDto.Id,
                    new UpdateReadTicketHistoryDto
                    {
                        ReadByClientDate = ticketHistoryDto?.ReadByClientDate ?? this._clock.Now,
                        ReadBySpecialistDate = ticketHistoryDto?.ReadBySpecialistDate
                    });
            }
        }

        var ticketStatuses = await this._ticketStatusAppService.GetListStatusesAsync();
        this.CreateTicketHistory = new CreateTicketHistoryDto
        {
            TicketStatusId = this.Ticket?.TicketStatusId ??
                             ticketStatuses.FirstOrDefault(x => x.Name.ToLowerInvariant() == TicketStatusNewLower)?.Id ??
                             ticketStatuses.FirstOrDefault()?.Id ?? Guid.Empty
        };
        this.MyTicket = await this._ticketStatusAppService.GetMyTicketStatusByResponsibleIdAsync(currentCustomerUserProfile?.Id ??
            Guid.Empty);
        await this._ticketsPublicAppService.MakeReadTicketByIdAsync(id, new UpdateReadTicketDto
        {
            ReadByClientDate = isClient ? this.Ticket?.ReadByClientDate ?? this._clock.Now : this.Ticket?.ReadByClientDate,
            ReadBySpecialistDate =
                isSpecialist ? this.Ticket?.ReadBySpecialistDate ?? this._clock.Now : this.Ticket?.ReadBySpecialistDate
        });
    }
}

public class TicketPreviewHistoryViewModel
{
    public int NextPage { get; set; }
    public int PrevPage { get; set; }
    public long TicketId { get; set; }
    public List<TicketHistoryDto> TicketHistoryDtos { get; set; } = new();
}
