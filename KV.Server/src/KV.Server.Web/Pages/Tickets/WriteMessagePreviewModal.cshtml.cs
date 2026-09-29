namespace KV.Server.Web.Pages.Tickets;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Timing;

public class WriteMessagePreviewModalModel : ServerPageModel
{
    private readonly IClock _clock;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly INotificationAppService _notificationAppService;
    private readonly ITicketMessageAppService _ticketMessageAppService;
    private readonly ITicketsAppService _ticketsAppService;

    public WriteMessagePreviewModalModel(ITicketMessageAppService ticketMessageAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        INotificationAppService notificationAppService,
        ITicketsAppService ticketsAppService,
        IClock clock)
    {
        this._ticketMessageAppService = ticketMessageAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._notificationAppService = notificationAppService;
        this._ticketsAppService = ticketsAppService;
        this._clock = clock;
    }

    [BindProperty(SupportsGet = true)] public long Id { get; set; }

    [BindProperty] public Guid CurrentCustomerUserProfileId { get; set; }

    [BindProperty] public WriteMessagePreviewViewModel WriteTicketMessage { get; set; }

    public async Task OnGetAsync()
    {
        if (this.CurrentUser?.Id == null)
        {
            throw new UserFriendlyException("Пользователя не существует");
        }

        var currentCustomerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser?.Id ?? Guid.Empty);
        this.CurrentCustomerUserProfileId = currentCustomerUserProfile.Id;
        this.WriteTicketMessage = new WriteMessagePreviewViewModel
        {
            TicketId = this.Id
        };
    }

    public async Task OnPostAsync()
    {
        var ticket = await this._ticketsAppService.GetTicketDetailsAsync(this.WriteTicketMessage.TicketId);
        await this._ticketMessageAppService.SendMessageAsync(new CreateUpdateTicketMessageDto
        {
            Comment = this.WriteTicketMessage.Message,
            TicketId = this.WriteTicketMessage.TicketId,
            CreationTime = this._clock.Now,
            CreatorCustomerUserProfileId = this.CurrentCustomerUserProfileId
        });
        await this._notificationAppService.SendNotificationAsync(new CreateUserNotificationDto
        {
            CreatorId = this.CurrentUser?.Id,
            Title = $"Сообщение по заявке {this.WriteTicketMessage?.TicketId}",
            Url = $"/tickets/detailsMessage/{this.WriteTicketMessage?.TicketId}",
            NotificationCategoryId =
                (await this._notificationAppService.GetFindNotificationCategoryByContainsLowerCaseNameAsync("заявк"))?.Id,
            IdentityUserId = ticket?.CreatorId ?? Guid.Empty,
            CreationTime = this._clock.Now
        });
    }
}

public class WriteMessagePreviewViewModel
{
    [Required] public string Message { get; set; }

    [HiddenInput] public long TicketId { get; set; }
}
