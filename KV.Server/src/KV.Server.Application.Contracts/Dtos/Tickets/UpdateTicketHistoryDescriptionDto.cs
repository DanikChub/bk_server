namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class UpdateTicketHistoryDescriptionDto : EntityDto<Guid>
{
    public string Description { get; set; }
}
