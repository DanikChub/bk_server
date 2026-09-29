namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class UserNotificationItemDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string CreatorFirstName { get; set; }
    public string CreatorLastName { get; set; }
    public DateTime? Seen { get; set; }
    public Guid IdentityUserId { get; set; }
    public string CategoryNotificationStyle { get; set; }
    public string CategoryNotificationName { get; set; }
    public Guid StatusNotificationId { get; set; }
}
