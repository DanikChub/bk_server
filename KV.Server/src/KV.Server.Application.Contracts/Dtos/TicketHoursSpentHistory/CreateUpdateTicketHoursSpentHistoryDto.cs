namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class CreateUpdateTicketHoursSpentHistoryDto : AuditedEntityDto<Guid>
{
    public int Spent { get; set; }
    public Guid ConstraintTypeId { get; set; }
    public Guid TicketHistoryId { get; set; }
}
