namespace KV.Server.Dtos.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KV.Server.Dtos.File;

public class CreateTicketHistoryDto
{
    [Required] public string Description { get; set; }

    public long TicketId { get; set; }
    public Guid TicketStatusId { get; set; }
    public Guid TicketHistoryId { get; set; }
    public Guid? CreatorId { get; set; }
    public TicketHistoryType Type { get; set; }
    public List<UploadedFileDto> Attachments { get; set; }
}
