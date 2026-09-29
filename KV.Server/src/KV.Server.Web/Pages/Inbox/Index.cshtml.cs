namespace KV.Server.Web.Pages.Inbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

[Authorize]
public class IndexModel : ServerPageModel
{
    private readonly INotificationAppService _notificationAppService;

    public PagedResultDto<UserNotificationItemDto> Notifcations { get; set; } = new();

    public List<NotificationCategoryDto> NotificationCategoriesView { get; set; } = new();

    public List<NotificationStatusInfoDto> NotificationStatusesView { get; set; } = new();

    public IndexModel(INotificationAppService notificationAppService)
    {
        this._notificationAppService = notificationAppService;
    }

    public int PrevPage { get; set; }
    public int NextPage { get; set; }

    [BindProperty(Name = "skipPage", SupportsGet = true)]
    public int CurrentPage { get; set; }

    [BindProperty(Name = "status", SupportsGet = true)]
    public Guid? NotificationStatusId { get; set; }

    [BindProperty(Name = "category", SupportsGet = true)]
    public Guid? NotificationCategoryId { get; set; }

    [BindProperty(Name = "title", SupportsGet = true)]
    public string Title { get; set; }

    public async Task OnGetAsync()
    {
        this.NotificationCategoriesView = await this._notificationAppService.GetNotificationCategoriesListAsync();
        this.NotificationStatusesView = await this._notificationAppService.GetNotificationStatusInfosListAsync(this.CurrentUser?.Id);
        var page = new GetNotificationListRequestDto
        {
            SkipCount = this.CurrentPage * LimitedResultRequestDto.DefaultMaxResultCount,
            Title = this.Title,
            NotificationCategoryId = this.NotificationCategoryId.ToString(),
            NotificationStatusId = this.NotificationStatusId.ToString(),
            IdentityUserId = this.CurrentUser?.Id.ToString()
        };
        var notificationCount = await this._notificationAppService
            .GetCountNotificationAsync(this.CurrentUser?.Id, this.NotificationCategoryId, this.NotificationStatusId);
        var maxPage = notificationCount / LimitedResultRequestDto.DefaultMaxResultCount;
        this.PrevPage = this.CurrentPage == 0 ? 0 : this.CurrentPage - 1;
        this.NextPage = this.CurrentPage < maxPage ? this.CurrentPage + 1 : maxPage;
        this.Notifcations = await this._notificationAppService.GetListAsync(page);
    }

    public async Task<IActionResult> OnGetRedirectAsync(Guid id)
    {
        var notification = await this._notificationAppService.GetAsync(id);
        if (notification != null && notification.IdentityUserId == this.CurrentUser.Id)
        {
            await this._notificationAppService.MakeReadNotificationAsync(id);
        }

        return this.Redirect(notification?.Url ?? "");
    }
}
