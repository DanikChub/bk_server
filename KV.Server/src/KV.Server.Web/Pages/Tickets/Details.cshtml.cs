namespace KV.Server.Web.Pages.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Dtos.Employees;
using KV.Server.Dtos.File;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using KV.Server.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Users;

[Authorize]
public class DetailsModel : ServerPageModel
{
    private readonly IClock _clock;
    private readonly IContractsAppService _contractsAppService;
    private readonly ICrudAnswerTemplateService _crudAnswerTemplateService;
    private readonly ICrudServicePackageService _crudServicePackageService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly IEventAppService _eventAppService;
    private readonly INotificationAppService _notificationAppService;
    private readonly ITicketFavoriteAppService _ticketFavoriteAppService;
    private readonly ITicketFileAppService _ticketFileAppService;
    private readonly ITicketHistoryAppService _ticketHistoryAppService;
    private readonly ITicketHoursSpentHistoryAppService _ticketHoursSpentHistoryAppService;
    private readonly ITicketMessageAppService _ticketMessageAppService;
    private readonly IEmployeeProfileAppService _employeeProfileAppService;
    private readonly ITicketsAppService _ticketsAppService;
    private readonly ITicketSectionAppService _ticketSectionAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;
    private readonly ITicketTypeAppService _ticketTypeAppService;

    public List<ConstraintTypeDto> ConstraintTypes = new();

    public ContractDto Contract = new();

    public List<EventDto> Events = new();

    public TicketStatusInfoDto MyTicket = new();

    public List<ServicePackageDto> TicketServicePackagesView = new();

    public List<TicketStatusInfoDto> TicketStatusesView = new();

    public DetailsModel(ITicketsAppService ticketsAppService,
        ITicketHistoryAppService ticketHistoryAppService,
        TicketFileAppService ticketFileAppService, // inject?
        ITicketStatusAppService ticketStatusAppService,
        ITicketMessageAppService ticketMessageAppService,
        ICrudServicePackageService crudServicePackageService,
        INotificationAppService notificationAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        ICrudAnswerTemplateService crudAnswerTemplateService,
        IContractsAppService contractsAppService,
        ITicketHoursSpentHistoryAppService ticketHoursSpentHistoryAppService,
        ITicketFavoriteAppService ticketFavoriteAppService,
        ITicketSectionAppService ticketSectionAppService,
        ITicketTypeAppService ticketTypeAppService,
        IEventAppService eventAppService,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IClock clock,
        IEmployeeProfileAppService employeeProfileAppService)
    {
        this._ticketsAppService = ticketsAppService;
        this._ticketHistoryAppService = ticketHistoryAppService;
        this._ticketFileAppService = ticketFileAppService;
        this._ticketStatusAppService = ticketStatusAppService;
        this._ticketMessageAppService = ticketMessageAppService;
        this._crudServicePackageService = crudServicePackageService;
        this._notificationAppService = notificationAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._crudAnswerTemplateService = crudAnswerTemplateService;
        this._contractsAppService = contractsAppService;
        this._ticketHoursSpentHistoryAppService = ticketHoursSpentHistoryAppService;
        this._ticketFavoriteAppService = ticketFavoriteAppService;
        this._ticketSectionAppService = ticketSectionAppService;
        this._ticketTypeAppService = ticketTypeAppService;
        this._eventAppService = eventAppService;
        this._currentTenant = currentTenant;
        this._currentUser = currentUser;
        this._clock = clock;
        _employeeProfileAppService = employeeProfileAppService;
    }

    [BindProperty(Name = "skipPage", SupportsGet = true)]
    public int CurrentPage { get; set; }

    [BindProperty] public Guid? TicketCreatorId { get; set; }

    [BindProperty] public TicketDetailsDto Ticket { get; set; }

    [BindProperty] public CreateTicketHistoryViewModel CreateTicketHistory { get; set; }

    [BindProperty] public List<UploadedFileDto> UploadedFilesTicket { get; set; }

    [BindProperty] public TicketHistoryViewModel TicketHistory { get; set; } = new();

    public List<SelectListItem> TicketStatuses { get; set; }

    public IEnumerable<SelectListItem> AnswerTemplates { get; set; }

    public IEnumerable<SelectListItem> TicketRanges { get; set; }

    public List<SelectListItem> TicketSections { get; set; }

    public List<SelectListItem> TicketTypes { get; set; }

    public List<SelectListItem> Specialists { get; set; }

    [BindProperty] public EmployeeProfileDto Specialist { get; set; }

    [BindProperty] public CustomerUserProfileDto CurrentCustomerUserProfile { get; set; }

    [BindProperty] public Guid? CurrentCustomerUserProfileId { get; set; }

    [BindProperty] public TicketMessageInfoViewModel TicketMessage { get; set; } = new();

    [BindProperty] public CreateRightMenuViewModel CreateRightMenu { get; set; } = new();

    public List<ServicePackageDto> TicketActiveServicePackages { get; set; }
    public bool IsTicketFavorite { get; set; }
    [BindProperty] public bool IsHiddenForClient { get; set; } = false;
    protected async Task Init(long id)
    {
        TicketStatuses = new List<SelectListItem>();
        this.TicketStatusesView = await this._ticketStatusAppService.GetTicketStatusByCreatorIdAndTenantAsync(this.CurrentUser?.Id);
        this.UploadedFilesTicket = await this._ticketFileAppService.GetPrivateTicketAttachmentsListAsync(id);
        this.TicketServicePackagesView = await this._crudServicePackageService.ToListAsync();
        this.Ticket = await this._ticketsAppService.GetTicketDetailsAsync(id);

        var favorit = await _ticketFavoriteAppService.GetIsFavoriteClientByTicketIdAsync(id);
        IsTicketFavorite = favorit;
        this.TicketCreatorId = this.Ticket.CreatorId;
        this.CurrentCustomerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser?.Id ?? Guid.Empty);
        this.Specialist = await this._ticketsAppService.GetSpecialistByTicketIdAsync(id);
        this.Contract = await this._contractsAppService.GetContractByTicketIdAsync(id);
        var isCurrentUserResponsibleSpecialist = this.CurrentUser.Id == this.Specialist?.IdentityUserId;
        this.ConstraintTypes = await this._ticketHoursSpentHistoryAppService.ToListTicketRangeByTicketIdAsync(id);
        this.AnswerTemplates = (await this._crudAnswerTemplateService.ToListAsync())
            .RenderToSelectList(x => x.Title, x => x.Description);
        this.TicketRanges = (await this._ticketHoursSpentHistoryAppService.ToListConstraintTypesAsync())
            .RenderToSelectList(x => x.Name, x => x.Id.ToString());
        this.Events = await this._eventAppService.GetListEventsByTicketIdAsync(id);

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
        foreach (var ticketHistoryDto in this.TicketHistory.TicketHistoryDtos)
        {
            ticketHistoryDto.Attachments =
                await this._ticketFileAppService.GetTicketHistoryAttachmentsListAsync(ticketHistoryDto.Id);
            
            if (isCurrentUserResponsibleSpecialist && ticketHistoryDto?.ReadBySpecialistDate == null)
            {
                await this._ticketHistoryAppService.MakeReadTicketHistoryByIdAsync(ticketHistoryDto.Id,
                    new UpdateReadTicketHistoryDto
                    {
                        ReadByClientDate = ticketHistoryDto?.ReadByClientDate,
                        ReadBySpecialistDate = ticketHistoryDto?.ReadBySpecialistDate ?? this._clock.Now
                    });
            }
        }

        this.TicketHistory.TicketId = this.Ticket?.Id ?? id;

        var statuses = (await this._ticketStatusAppService.GetTicketStatusByCreatorIdAndTenantAsync(TicketCreatorId));


        foreach (var item in statuses)
        {
            TicketStatuses.Add(new SelectListItem { Text = item.DisplayName, Value = item.Id.ToString(), Selected = item.Id == Ticket.TicketStatusId });
        }
        this.CreateTicketHistory = new CreateTicketHistoryViewModel
        {
            TicketStatusId = this.Ticket?.TicketStatusId ?? Guid.Empty,
            TicketId = this.Ticket?.Id ?? id
        };
        this.CurrentCustomerUserProfileId = this.CurrentCustomerUserProfile?.Id ?? Guid.Empty;
        this.MyTicket = await this._ticketStatusAppService.GetMyTicketStatusByResponsibleIdAsync(this.CurrentCustomerUserProfileId);
        this.TicketMessage = new TicketMessageInfoViewModel
        {
            IsExistMessage = await this._ticketMessageAppService.ExistAnyTicketMessageAsync(id)
        };
        if (this.Ticket.ReadBySpecialistDate == null && isCurrentUserResponsibleSpecialist)
        {
                await this._ticketsAppService.MakeReadTicketByIdAsync(id, new UpdateReadTicketDto
                {
                    ReadByClientDate = this.Ticket.ReadByClientDate,
                    ReadBySpecialistDate = this._clock.Now
                });
        }

        this.TicketSections = (await this._ticketSectionAppService.GetTicketSectionsAsync())
            .RenderToSelectList(x => x.Name, x => x.Id)
            .ToList();

        this.TicketTypes = (await this._ticketTypeAppService.GetTicketTypesAsync())
            .RenderToSelectList(x => x.Name, x => x.Id)
            .ToList();

        this.Specialists = (await this._employeeProfileAppService.GetAllEmployeeAsync())
            .OrderBy(x => x.LastName)
            .Where(x => !(x.LastName.IsNullOrWhiteSpace() && x.FirstName.IsNullOrWhiteSpace() &&
                          x.MiddleName.IsNullOrWhiteSpace()))
            .RenderToSelectList(x => $"{x.LastName} {x.FirstName} {x.MiddleName}", x => x.Id.ToString())
            .ToList();

        this.Specialists.AddFirst(new SelectListItem { Text = "Отсутствует", Value = null });
        this.TicketActiveServicePackages =
            await this._contractsAppService.GetListActiveServicePackagesByTenantIdAsync(this.Ticket.TenantId);
        this.CreateRightMenu = new CreateRightMenuViewModel
        {
            TicketId = this.Ticket.Id,
            EventTicketId = this.Ticket.Id,
            EventTenantId = this.Ticket.TenantId,
            TicketTypeId = this.Ticket.TicketTypeId,
            TicketSectionId = this.Ticket.TicketSectionId,
            ResponsibleId = this.Ticket.ResponsibleId,
            TicketStatusId = this.Ticket.TicketStatusId
        };
    }
    public async Task<IActionResult> OnGetAsync(long id)
    {
        await Init(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var ticketHistory = await this._ticketHistoryAppService.CreateAdminTicketHistoryAsync(new CreateAdminTicketHistoryDto
        {
            Description = this.CreateTicketHistory.Description,
            CreatorId = this._currentUser?.Id,
            TicketStatusId = this.CreateTicketHistory.TicketStatusId,
            TicketId = this.CreateTicketHistory.TicketId,
            ConstraintTypeId = this.CreateTicketHistory.ConstraintTypeId,
            TicketHoursSpentHistoryCount = this.CreateTicketHistory.TicketHoursSpentHistoryCount,
            IsInternal = this.IsHiddenForClient
        });
        if (this.TicketCreatorId != null && this.TicketCreatorId != this._currentUser?.Id)
        {
            await this._notificationAppService.SendNotificationAsync(new CreateUserNotificationDto
            {
                CreatorId = this.CurrentUser.Id,
                Title = $"Ответ по заявке {this.CreateTicketHistory?.TicketId}",
                Url = $"/tickets/details/{this.CreateTicketHistory?.TicketId}",
                NotificationCategoryId =
                    (await this._notificationAppService.GetFindNotificationCategoryByContainsLowerCaseNameAsync("заявк"))
                    ?.Id,
                IdentityUserId = this.TicketCreatorId ?? Guid.Empty
            });
        }
        var ticket = await _ticketsAppService.GetTicketDetailsAsync(Ticket.Id);
        if (this.HttpContext.Request.Form.Files.Count != 0)
        {
            var formFiles = this.HttpContext.Request.Form.Files;
            foreach (var formFile in formFiles)
            {
                await this._ticketFileAppService.UploadTicketHistoryAttachmentAsync(
                    ticketHistory.Id,
                    new RemoteStreamContent(formFile.OpenReadStream(),
                        formFile.FileName, formFile.ContentType), ticket.TenantId);
            }
        }

        if (this.CreateTicketHistory.ConstraintTypeValue != 0)
        {
            await this._ticketsAppService.UpdateTicketConstraintByTicketIdAsync(this.CreateTicketHistory.TicketId, this.CreateTicketHistory.ConstraintTypeId, this.CreateTicketHistory.ConstraintTypeValue);
        }

        return this.Redirect($"{this.CreateTicketHistory.TicketId}");
    }

    public async Task<IActionResult> OnPostRightNavMenuAsync()
    {
        try
        {
            if (this.CreateRightMenu.EventName != null)
            {
                await this._eventAppService.CreateEventAsync(new CreateEventInTicketDto
                {
                    EventType = EventType.None,
                    EventDate = this.CreateRightMenu.EventDate,
                    Description = this.CreateRightMenu.EventName,
                    TicketId = this.CreateRightMenu.EventTicketId,
                    TenantId = this.CreateRightMenu.EventTenantId,
                    Title = this.CreateRightMenu.EventName
                });
            }

            await this._ticketsAppService.UpdateTicketStatusByTicketIdAsync(this.CreateRightMenu.TicketId,
                this.CreateRightMenu.TicketStatusId);
            await this._ticketsAppService.UpdateTicketSectionByTicketIdAsync(this.CreateRightMenu.TicketId,
                this.CreateRightMenu.TicketSectionId);
            await this._ticketsAppService.AssignedSpecialistAsync(this.CreateRightMenu.TicketId, this.CreateRightMenu.ResponsibleId);
            await this._ticketsAppService.UpdateTicketTypeByTicketIdAsync(this.CreateRightMenu.TicketId,
                this.CreateRightMenu.TicketTypeId);
            await this._ticketsAppService.UpdateTicketConstraintByTicketIdAsync(this.CreateRightMenu.TicketId,
                this.CreateRightMenu.ConstraintTypeId, this.CreateRightMenu.ConstraintTypeValue);
            return this.Redirect($"/Tickets/Details/{this.CreateRightMenu.TicketId}");
        }
        catch (UserFriendlyException exc)
        {
            Alerts.Danger(exc.Message, "Ошибка");
            await Init(this.CreateRightMenu.TicketId);
            return Page();
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
    public async Task<IActionResult> OnGetDeleteFavoriteAsync(long ticketId)
    {
        if (this._currentTenant?.Id == null)
        {
            await this._ticketFavoriteAppService.DeleteFavoriteSpecialistAsync(ticketId);
        }
        else
        {
            await this._ticketFavoriteAppService.DeleteFavoriteClientAsync(ticketId);
        }

        return this.NoContent();
    }
    public async Task<IActionResult> OnGetTakeResponsibleAsync(long id)
    {
        this.CurrentCustomerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser.Id ?? Guid.Empty);
        if (this.CurrentCustomerUserProfile?.Id == null)
        {
            if (this.CurrentUser?.Id == null)
            {
                throw new UserFriendlyException("Вы не завершили регистрацию");
            }

            this.CurrentCustomerUserProfile =
                await this._customerUserProfilesAppService.CreateByIdentityIdAsync(this.CurrentUser.Id ?? Guid.Empty);
        }

        this.Specialist = await this._ticketsAppService.AssignedSpecialistAsync(id, this.CurrentCustomerUserProfile.Id);
        return this.NoContent();
    }

    public async Task<IActionResult> OnGetDropResponsibleAsync(long id)
    {
        this.CurrentCustomerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser.Id ?? Guid.Empty);
        await this._ticketsAppService.RemoveAssignedSpecialistAsync(id, this.CurrentCustomerUserProfile.Id);
        this.Specialist = null;
        return this.NoContent();
    }

    public async Task<IActionResult> OnGetDownloadAllFileAsync(long id, Guid tenantId)
    {
        try
        {
            var remoteStream = await this._ticketFileAppService.GetDownloadAllFileInZipByTicketIdAsync(id, tenantId);
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
            var remoteStream = await this._ticketFileAppService.GetDownloadZipByTicketIdAsync(id);
            return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
        }
        catch
        {
            return this.NoContent();
        }
    }

    public async Task<IActionResult> OnGetDownloadTicketHistoryZipFileAsync(Guid ticketHistoryId, Guid tenantId)
    {
        try
        {
            var remoteStream = await this._ticketFileAppService.GetDownloadZipByTicketHistoryIdAsync(ticketHistoryId, tenantId);
            return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
        }
        catch
        {
            return this.NoContent();
        }
    }

    public async Task<IActionResult> OnGetDownloadOneFileAsync(Guid fileId, Guid tenantId)
    {
        try
        {
            var remoteStream = await this._ticketFileAppService.GetDownloadAttachmentAsync(fileId, tenantId);
            return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
        }
        catch
        {
            return this.NoContent();
        }
    }
}

public class CreateTicketHistoryViewModel
{
    [Required(ErrorMessage = "Описание обязателно для заполнения")]
    [MinLength(3, ErrorMessage = "Минимальная длина должна быть 3 символа")]
    public string Description { get; set; }

    public long TicketId { get; set; }
    public int TicketHoursSpentHistoryCount { get; set; }
    public Guid ConstraintTypeId { get; set; }
    public int ConstraintTypeValue { get; set; }
    public Guid TicketStatusId { get; set; }
    public Guid? AnswerTemplateId { get; set; }
    public Guid? CreatorId { get; set; }
}

public class CreateRightMenuViewModel
{
    public long TicketId { get; set; }

    public string EventName { get; set; }
    public DateTime EventDate { get; set; } = DateTime.Now.Date;
    public Guid? EventTenantId { get; set; }
    public long EventTicketId { get; set; }

    public Guid ConstraintTypeId { get; set; }
    public int ConstraintTypeValue { get; set; }

    public Guid TicketTypeId { get; set; }
    public Guid TicketStatusId { get; set; }
    public Guid? ResponsibleId { get; set; }
    public Guid? TicketSectionId { get; set; }
}

public class TicketHistoryViewModel
{
    public int NextPage { get; set; }
    public int PrevPage { get; set; }
    public long TicketId { get; set; }
    public List<TicketHistoryDto> TicketHistoryDtos { get; set; }
}

public class TicketMessageInfoViewModel
{
    public bool IsExistMessage { get; set; }
}
