namespace KV.Server.PublicWeb.Pages.Tickets;
using KV.Server.Dtos.File;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

[Authorize]
public class DetailsModel : ServerPageModel
{
    public const string TicketStatusNewLower = "new";
    public const string TicketStatusCloseLower = "answered";
    private readonly IClock _clock;
    private readonly IContractsAppService _contractsAppService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly INotificationAppService _notificationAppService;
    private readonly ITicketFavoriteAppService _ticketFavoriteAppService;
    private readonly ITicketFileAppService _ticketFileAppService;
    private readonly ITicketHistoryAppService _ticketHistoryAppService;

    private readonly ITicketsPublicAppService _ticketsPublicAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;

    public TicketStatusInfoDto FavoriteTicket { get; set; } = new();

    public TicketStatusInfoDto MyTicket { get; set; } = new();

    public List<ServicePackageDto> TicketActiveServicePackagesView { get; set; } = new();

    public List<ServicePackageDto> TicketNotActiveServicePackagesView { get; set; } = new();

    public List<TicketStatusInfoDto> TicketStatusesView { get; set; } = new();

    public DetailsModel(ITicketsPublicAppService ticketsPublicAppService,
        ITicketHistoryAppService ticketHistoryAppService,
        ITicketFileAppService ticketFileAppService,
        ITicketStatusAppService ticketStatusAppService,
        INotificationAppService notificationAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        IContractsAppService contractsAppService,
        ITicketFavoriteAppService ticketFavoriteAppService,
        IClock clock,
        ICurrentTenant currentTenant)
    {
        this._ticketsPublicAppService = ticketsPublicAppService;
        this._ticketHistoryAppService = ticketHistoryAppService;
        this._ticketFileAppService = ticketFileAppService;
        this._ticketStatusAppService = ticketStatusAppService;
        this._notificationAppService = notificationAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._contractsAppService = contractsAppService;
        this._ticketFavoriteAppService = ticketFavoriteAppService;
        this._clock = clock;
        this._currentTenant = currentTenant;
    }

    [BindProperty(Name = "skipPage", SupportsGet = true)]
    public int CurrentPage { get; set; }

    [BindProperty] public Guid? TicketCreatorId { get; set; }

    public TicketDetailsDto Ticket { get; set; } = new();

    [BindProperty] public List<UploadedFileDto> UploadedFilesTicket { get; set; } = new();

    [BindProperty] public CreateTicketHistoryDto CreateTicketHistory { get; set; } = new();

    [BindProperty] public TicketHistoryViewModel TicketHistory { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(long id)
    {
        this.TicketStatusesView = await this._ticketStatusAppService.GetTicketStatusByCreatorIdAndTenantAsync(this.CurrentUser?.Id);

        this.UploadedFilesTicket = await this._ticketFileAppService.GetTicketAttachmentsListAsync(id);
        this.TicketActiveServicePackagesView =
            await this._contractsAppService.GetListActiveServicePackagesByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty);
        this.TicketNotActiveServicePackagesView =
            await this._contractsAppService.GetListNotActiveServicePackagesByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty);
        this.Ticket = await this._ticketsPublicAppService.GetTicketDetailsAsync(id);
        if (this.Ticket == null)
        {
            return this.Redirect("/tickets");
        }

        var ticketStatuses = await this._ticketStatusAppService.GetListStatusesAsync();
        var currentTicketStatus = ticketStatuses.FirstOrDefault(x => x.Id == this.Ticket.TicketStatusId);
        if (currentTicketStatus?.IsPublic == false)
        {
            return this.Redirect($"/tickets/editcompose?id={id}");
        }

        this.TicketCreatorId = this.Ticket?.CreatorId;
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

        this.CreateTicketHistory = new CreateTicketHistoryDto
        {
            TicketStatusId = this.Ticket?.TicketStatusId ??
                             ticketStatuses.FirstOrDefault(x => x.Name.ToLowerInvariant()
                                == TicketStatusNewLower)?.Id ??
                             ticketStatuses.FirstOrDefault()?.Id ?? Guid.Empty
        };
        this.MyTicket = await this._ticketStatusAppService.GetMyTicketStatusByResponsibleIdAsync(currentCustomerUserProfile?.Id ??
            Guid.Empty);
        this.FavoriteTicket = await this._ticketStatusAppService.GetTicketFavoriteInfoAsync();
        if (this.Ticket?.ReadByClientDate == null || this.Ticket?.ReadBySpecialistDate == null)
        {
            if (isClient || isSpecialist)
            {
                await this._ticketsPublicAppService.MakeReadTicketByIdAsync(id, new UpdateReadTicketDto
                {
                    ReadByClientDate = isClient ? this.Ticket?.ReadByClientDate ?? this._clock.Now : this.Ticket?.ReadByClientDate,
                    ReadBySpecialistDate = isSpecialist
                        ? this.Ticket?.ReadBySpecialistDate ?? this._clock.Now
                        : this.Ticket?.ReadBySpecialistDate
                });
            }
        }

        return this.Page();
    }

    public async Task<IActionResult> OnGetDownloadAllFileAsync(long id)
    {
        try
        {
            var remoteStream = await this._ticketFileAppService.GetPublicDownloadAllFileInZipByTicketIdAsync(id);
            return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
        }
        catch
        {
            return this.NoContent();
        }
    }

    public async Task<IActionResult> OnGetDownloadFileTicketZipAsync(long id)
    {
        this.UploadedFilesTicket = await this._ticketFileAppService.GetTicketAttachmentsListAsync(id);
        if (this.UploadedFilesTicket.Count == 0)
        {
            return this.NoContent();
        }

        try
        {
            var remoteStream = await this._ticketFileAppService.GetPublicDownloadAllFileInZipByTicketIdAsync(id);
            return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
        }
        catch
        {
            return this.NoContent();
        }
    }

    public async Task<IActionResult> OnGetDownloadTicketHistoryZipFileAsync(Guid ticketHistoryId)
    {
        try
        {
            var remoteStream = await this._ticketFileAppService.GetPublicDownloadZipByTicketHistoryIdAsync(ticketHistoryId);
            return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
        }
        catch
        {
            return this.NoContent();
        }
    }

    public async Task<IActionResult> OnGetDownloadOneFileAsync(Guid fileId)
    {
        try
        {
            var remoteStream = await this._ticketFileAppService.GetPublicDownloadAttachmentAsync(fileId);
            return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
        }
        catch
        {
            return this.NoContent();
        }
    }

    public async Task<IActionResult> OnGetSetFavoriteAsync(long ticketId)
    {
        if (this._currentTenant?.Id == null)
        {
            await this._ticketFavoriteAppService.SetFavoriteSpecialistAsync(ticketId);
        }
        else
        {
            await this._ticketFavoriteAppService.SetFavoriteClientAsync(ticketId);
        }

        return this.NoContent();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        this.CreateTicketHistory.CreatorId = this.CurrentUser?.Id;
        var ticketHistory = await this._ticketsPublicAppService.CreateTicketHistoryAsync(this.CreateTicketHistory);
        if (this.TicketCreatorId != null && this.TicketCreatorId != this.CurrentUser?.Id)
        {
            await this._notificationAppService.SendNotificationAsync(new CreateUserNotificationDto
            {
                CreatorId = this.CreateTicketHistory.CreatorId,
                Title = $"Ответ по заявке {this.CreateTicketHistory?.TicketId}",
                Url = $"/tickets/details/{this.CreateTicketHistory?.TicketId}",
                NotificationCategoryId =
                    (await this._notificationAppService.GetFindNotificationCategoryByContainsLowerCaseNameAsync("заявк"))
                    ?.Id,
                IdentityUserId = this.TicketCreatorId ?? Guid.Empty
            });
        }

        if (this.HttpContext.Request.Form.Files.Count != 0)
        {
            var formFiles = this.HttpContext.Request.Form.Files;
            foreach (var formFile in formFiles)
            {
                await this._ticketFileAppService.UploadPublicTicketHistoryAttachmentAsync(
                    ticketHistory.Id,
                    new RemoteStreamContent(formFile.OpenReadStream(),
                        formFile.FileName, formFile.ContentType));
            }
        }

        return this.Redirect($"{this.CreateTicketHistory?.TicketId}");
    }
}

public class TicketHistoryViewModel
{
    public int NextPage { get; set; }
    public int PrevPage { get; set; }
    public long TicketId { get; set; }
    public bool IsInternal { get; set; }
    public List<TicketHistoryDto> TicketHistoryDtos { get; set; } = new();
}
