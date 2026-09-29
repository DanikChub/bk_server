namespace KV.Server;
using Volo.Abp.Application.Dtos;

public class GetNotificationListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
    public string Title { get; set; }
    public string IdentityUserId { get; set; }
    public string NotificationStatusId { get; set; }
    public string NotificationCategoryId { get; set; }
}
