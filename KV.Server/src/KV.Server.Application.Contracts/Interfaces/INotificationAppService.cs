namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface INotificationAppService : IApplicationService
{
    Task<PagedResultDto<UserNotificationItemDto>> GetListAsync(GetNotificationListRequestDto input);
    Task<int> GetCountNotificationAsync(Guid? identityUserId, Guid? categoryId, Guid? notificationStatusId);
    Task<List<NotificationCategoryDto>> GetNotificationCategoriesListAsync();
    Task<List<NotificationStatusInfoDto>> GetNotificationStatusInfosListAsync(Guid? identityUserId);
    Task SendNotificationAsync(CreateUserNotificationDto create);
    Task<NotificationCategoryDto> GetFindNotificationCategoryByContainsLowerCaseNameAsync(string name);
    Task<UserNotificationItemDto> GetAsync(Guid id);
    Task MakeReadNotificationAsync(Guid notificationId);
}
