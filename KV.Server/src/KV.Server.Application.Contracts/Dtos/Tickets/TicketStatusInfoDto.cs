namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class TicketStatusInfoDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public bool IsPublic { get; set; }
    public string DisplayName { get; set; }
    public string Style { get; set; }
    public string DisplayNameMany { get; set; }
    public string Icon { get; set; }
    public long Count { get; set; }
}
