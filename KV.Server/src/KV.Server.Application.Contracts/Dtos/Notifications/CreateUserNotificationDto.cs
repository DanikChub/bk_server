namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class CreateUserNotificationDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; }
    public string Url { get; set; }
    public Guid IdentityUserId { get; set; }
    public Guid? NotificationCategoryId { get; set; }
}
