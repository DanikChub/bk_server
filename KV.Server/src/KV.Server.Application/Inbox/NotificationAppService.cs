namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Notifications;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

public class NotificationAppService : ApplicationService, INotificationAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<IdentityUser, Guid> _identityUserRepository;
    private readonly IRepository<NotificationCategory, Guid> _notificationCategoryRepository;
    private readonly IRepository<NotificationStatus, Guid> _notificationStatusRepository;
    private readonly IRepository<UserNotification, Guid> _userNotificationRepository;

    public NotificationAppService(IRepository<UserNotification, Guid> userNotificationRepository,
        IRepository<NotificationCategory, Guid> notificationCategoryRepository,
        IRepository<IdentityUser, Guid> identityUserRepository,
        IRepository<NotificationStatus, Guid> notificationStatusRepository,
        IDataFilter dataFilter)
    {
        this._userNotificationRepository = userNotificationRepository;
        this._notificationCategoryRepository = notificationCategoryRepository;
        this._identityUserRepository = identityUserRepository;
        this._notificationStatusRepository = notificationStatusRepository;
        this._dataFilter = dataFilter;
    }

    public async Task<PagedResultDto<UserNotificationItemDto>> GetListAsync(GetNotificationListRequestDto input)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._userNotificationRepository
                .WithDetailsAsync(x => x.NotificationCategory);

            var notificationStatusEnum = NotificationStatuses.UnRead;
            if (Guid.TryParse(input.NotificationStatusId, out var notificationStatus))
            {
                var status = await this._notificationStatusRepository.FirstOrDefaultAsync(x => x.Id == notificationStatus);
                notificationStatusEnum = status.Status;
            }

            Guid? identityUserId = input.IdentityUserId.IsNullOrWhiteSpace() ? null : Guid.Parse(input.IdentityUserId);
            Guid? categoryId = input.NotificationCategoryId.IsNullOrWhiteSpace()
                ? null
                : Guid.Parse(input.NotificationCategoryId);
            query = FilterByStatusUserNotification(query, identityUserId, categoryId, notificationStatusEnum);
            if (!string.IsNullOrWhiteSpace(input.Title))
            {
                query = query.Where(x => x.Title.Contains(input.Title.Trim()));
            }

            if (!string.IsNullOrEmpty(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }

            var totalCount = query.Count();
            var entities = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount == 0 ? GetNotificationListRequestDto.DefaultPageSize : input.MaxResultCount)
                .ToList();
            var dtos = this.ObjectMapper.Map<List<UserNotification>, List<UserNotificationItemDto>>(entities);
            foreach (var dto in dtos)
            {
                if (dto.CreatorId != null)
                {
                    var creator = await this._identityUserRepository.FirstOrDefaultAsync(x => x.Id == dto.CreatorId);
                    dto.CreatorFirstName = creator.Name;
                    dto.CreatorLastName = creator.Surname;
                }
            }

            return new PagedResultDto<UserNotificationItemDto>(totalCount, dtos);
        }
    }

    public async Task<int> GetCountNotificationAsync(Guid? identityUserId, Guid? categoryId, Guid? notificationStatusId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            if (identityUserId == null)
            {
                return 0;
            }

            var notificationStatusEnum = NotificationStatuses.UnRead;
            if (notificationStatusId != null)
            {
                var status = await this._notificationStatusRepository.FirstOrDefaultAsync(x => x.Id == notificationStatusId);
                notificationStatusEnum = status.Status;
            }

            var query = await this._userNotificationRepository.WithDetailsAsync(x => x.NotificationCategory);
            query = FilterByStatusUserNotification(query, identityUserId, categoryId, notificationStatusEnum);
            var count = query.Count();
            return count;
        }
    }

    public async Task<List<NotificationCategoryDto>> GetNotificationCategoriesListAsync()
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var entities = await this._notificationCategoryRepository.ToListAsync();
            var dtos = this.ObjectMapper.Map<List<NotificationCategory>, List<NotificationCategoryDto>>(entities);
            return dtos;
        }
    }

    public async Task<List<NotificationStatusInfoDto>> GetNotificationStatusInfosListAsync(Guid? identityUserId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._userNotificationRepository.GetQueryableAsync();
            var notificationStatuses = await this._notificationStatusRepository.ToListAsync();
            var notificationStatusInfos = new List<NotificationStatusInfoDto>();
            foreach (var status in notificationStatuses)
            {
                var dto = this.ObjectMapper.Map<NotificationStatus, NotificationStatusInfoDto>(status);
                dto.Count = FilterByStatusUserNotification(query, identityUserId, null, status.Status)
                    .Count();
                notificationStatusInfos.Add(dto);
            }

            return notificationStatusInfos;
        }
    }

    public async Task SendNotificationAsync(CreateUserNotificationDto create)
    {
        var notification = this.ObjectMapper.Map<CreateUserNotificationDto, UserNotification>(create);
        await this._userNotificationRepository.InsertAsync(notification);
    }

    public async Task<NotificationCategoryDto> GetFindNotificationCategoryByContainsLowerCaseNameAsync(string name)
    {
        var category = await this._notificationCategoryRepository.FirstOrDefaultAsync(x => x.Name.ToLower().Contains(name.ToLower()));
        var dto = this.ObjectMapper.Map<NotificationCategory, NotificationCategoryDto>(category);
        return dto;
    }

    public async Task<UserNotificationItemDto> GetAsync(Guid id)
    {
        var notification = await this._userNotificationRepository.FirstOrDefaultAsync(x => x.Id == id);
        var dto = this.ObjectMapper.Map<UserNotification, UserNotificationItemDto>(notification);
        return dto;
    }

    public async Task MakeReadNotificationAsync(Guid notificationId)
    {
        var notification = await this._userNotificationRepository.FirstOrDefaultAsync(x => x.Id == notificationId);
        notification.Seen = DateTime.Now;
        await this._userNotificationRepository.UpdateAsync(notification);
    }

    private static IQueryable<UserNotification> FilterByStatusUserNotification(IQueryable<UserNotification> query,
        Guid? identityUserId, Guid? categoryId, NotificationStatuses notificationStatus)
    {
        if (notificationStatus == NotificationStatuses.Read)
        {
            query = query.Where(x => x.IdentityUserId == identityUserId);
            query = query.Where(x => x.Seen != null);
        }

        if (notificationStatus == NotificationStatuses.UnRead)
        {
            query = query.Where(x => x.IdentityUserId == identityUserId);
            query = query.Where(x => x.Seen == null);
        }

        if (categoryId != null)
        {
            query = query.Where(x => x.NotificationCategoryId == categoryId);
        }

        return query;
    }
}
