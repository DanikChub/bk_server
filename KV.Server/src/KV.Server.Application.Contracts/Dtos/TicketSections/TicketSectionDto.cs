namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class TicketSectionDto : EntityDto<Guid>
{
    public string Name { get; set; }
}
