namespace KV.Server.Dtos.Tickets;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

public class TicketListItemDto : FullAuditedEntityDto<long>
{
    public string Code { get; set; }
    public string Subject { get; set; }
    public string CreatorFirstName { get; set; }
    public string CreatorLastName { get; set; }
    public string ResponsibleLastName { get; set; }

    public string TicketStatusName { get; set; }
    public string TicketStatusDisplayName { get; set; }
    public string TicketStatusStyle { get; set; }
    public string TicketTypeName { get; set; }
    public string TicketSectionName { get; set; }
    public string CreatorFullName { get; set; }
    public string ResponsibleFullName { get; set; }
    public string CustomerShortName { get; set; }
    public DateTime DueDate { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime? ReadByClientDate { get; set; }
    public DateTime? ReadBySpecialistDate { get; set; }
    public string LastTagName { get; set; }
    public string ServicePackageName { get; set; }
    public string ServicePackageStyle { get; set; }
}
