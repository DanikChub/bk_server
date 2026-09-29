namespace KV.Server.Dtos.Tickets;
using System;
using Volo.Abp.Application.Dtos;

public class TicketTypeDto : EntityDto<Guid>
{
    public string Name { get; set; }
}
