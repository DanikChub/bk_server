namespace KV.Server.Web.Pages.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Timing;

[Authorize]
public class DetailsMessageModel : ServerPageModel
{
    private readonly IClock _clock;
    private readonly ICrudServicePackageService _crudServicePackageService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly INotificationAppService _notificationAppService;
    private readonly ITicketMessageAppService _ticketMessageAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;

    public TicketStatusInfoDto MyTicket = new();

    public List<ServicePackageDto> TicketServicePackagesView = new();

    public List<TicketStatusInfoDto> TicketStatusesView = new();

    public DetailsMessageModel(ITicketMessageAppService ticketMessageAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        INotificationAppService notificationAppService,
        ITicketStatusAppService ticketStatusAppService,
        ICrudServicePackageService crudServicePackageService,
        IClock clock)
    {
        this._ticketMessageAppService = ticketMessageAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._notificationAppService = notificationAppService;
        this._ticketStatusAppService = ticketStatusAppService;
        this._crudServicePackageService = crudServicePackageService;
        this._clock = clock;
    }

    [BindProperty(Name = "skipPage", SupportsGet = true)]
    public int CurrentPage { get; set; }

    [BindProperty] public Guid CurrentCustomerUserProfileId { get; set; }

    [BindProperty] public TicketMessageViewModel TicketMessage { get; set; }

    [BindProperty] public WriteTicketMessageViewModel WriteTicketMessage { get; set; }

    public async Task OnGetAsync(long id)
    {
        this.TicketStatusesView = await this._ticketStatusAppService.GetTicketStatusByCreatorIdAndTenantAsync(this.CurrentUser?.Id);
        this.TicketServicePackagesView = await this._crudServicePackageService.ToListAsync();
        this.TicketMessage = new TicketMessageViewModel();
        this.WriteTicketMessage = new WriteTicketMessageViewModel();
        var currentCustomerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser?.Id ?? Guid.Empty);
        this.CurrentCustomerUserProfileId = currentCustomerUserProfile?.Id ?? Guid.Empty;
        if (currentCustomerUserProfile == null)
        {
            throw new UserFriendlyException("Пользователя не существует");
        }

        var messages = await this._ticketMessageAppService.GetListAsync(new GetTicketMessagesListRequestDto
        {
            TicketId = id,
            MaxResultCount = 10,
            SkipCount = LimitedResultRequestDto.DefaultMaxResultCount * this.CurrentPage,
            Sorting = "CreationTime ASC"
        });
        var ticketMessageCount = await this._ticketMessageAppService.GetCountTicketMessageAsync(id);
        var maxPage = (ticketMessageCount - 1) / LimitedResultRequestDto.DefaultMaxResultCount;
        this.TicketMessage.PrevPage = this.CurrentPage == 0 ? 0 : this.CurrentPage - 1;
        this.TicketMessage.NextPage = this.CurrentPage < maxPage ? this.CurrentPage + 1 : maxPage;
        this.TicketMessage.Messages = messages.Items.ToList();
        this.WriteTicketMessage.TicketId = id;
        this.MyTicket = await this._ticketStatusAppService.GetMyTicketStatusByResponsibleIdAsync(currentCustomerUserProfile?.Id);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await this._ticketMessageAppService.SendMessageAsync(new CreateUpdateTicketMessageDto
        {
            Comment = this.WriteTicketMessage.Message,
            TicketId = this.WriteTicketMessage.TicketId,
            CreationTime = this._clock.Now,
            CreatorCustomerUserProfileId = this.CurrentCustomerUserProfileId
        });
        var currentUserId = this.CurrentUser?.Id;
        return this.Redirect($"/tickets/detailsMessage/{this.WriteTicketMessage?.TicketId}?skipPage={this.CurrentPage}");
    }
}

public class WriteTicketMessageViewModel
{
    [Required] public string Message { get; set; }

    [HiddenInput] public long TicketId { get; set; }
}

public class TicketMessageViewModel
{
    public List<TicketMessageDto> Messages = new();
    public int PrevPage { get; set; }
    public int NextPage { get; set; }
}
