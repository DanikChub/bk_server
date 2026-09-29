namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class NotificationStatusInfoDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Style { get; set; }
    public string DisplayNameMany { get; set; }
    public string Icon { get; set; }
    public NotificationStatuses NotificationStatusEnum { get; set; }
    public int Count { get; set; }
}
