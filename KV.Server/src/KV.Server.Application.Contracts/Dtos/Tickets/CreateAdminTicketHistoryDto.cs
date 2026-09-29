namespace KV.Server;
using System;

public class CreateAdminTicketHistoryDto
{
    public string Description { get; set; }
    public long TicketId { get; set; }
    public int TicketHoursSpentHistoryCount { get; set; }
    public TicketHistoryType Type { get; set; }
    public Guid ConstraintTypeId { get; set; }
    public Guid TicketStatusId { get; set; }
    public Guid AnswerTemplateId { get; set; }
    public Guid? CreatorId { get; set; }
    public bool IsInternal { get; set; }
}
