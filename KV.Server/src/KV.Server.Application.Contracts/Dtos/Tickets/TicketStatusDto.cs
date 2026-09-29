namespace KV.Server.Dtos.Tickets;
using System;
using Volo.Abp.Application.Dtos;

public class TicketStatusDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public bool IsPublic { get; set; }
    public string DisplayName { get; set; }
    public string Style { get; set; }
    public string DisplayNameMany { get; set; }
    public string Icon { get; set; }
}
