namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class TicketHoursSpentHistoryDto : AuditedEntityDto<Guid>
{
    public int Spent { get; set; }
    public string ConstraintTypeName { get; set; }
    public Guid TicketHistoryId { get; set; }
}
