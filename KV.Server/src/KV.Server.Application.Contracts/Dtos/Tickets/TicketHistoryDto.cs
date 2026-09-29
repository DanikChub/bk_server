namespace KV.Server.Dtos.Tickets;
using System;
using System.Collections.Generic;
using KV.Server.Dtos.File;
using Volo.Abp.Application.Dtos;

public class TicketHistoryDto : FullAuditedEntityDto<Guid>
{
    public string Description { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string UserName { get; set; }
    public TicketHistoryType Type { get; set; }
    public DateTime? ReadByClientDate { get; set; }
    public DateTime? ReadBySpecialistDate { get; set; }
    public bool IsSpecialistMessage { get; set; }
    public List<UploadedFileDto> Attachments { get; set; }
    public bool IsInternal { get; set; }
}
