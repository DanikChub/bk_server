namespace KV.Server.Dtos.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KV.Server.Dtos.File;

public class CreateUpdateTicketDto
{
    [Required] public string Subject { get; set; }

    [Required] public string Description { get; set; }

    public Guid TicketTypeId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid TicketStatusId { get; set; }
    public Guid? TicketSectionId { get; set; }
    public Guid CreatorId { get; set; }
    public Guid ContractId { get; set; }
    public List<UploadedFileDto> Attachments { get; set; }
    public DateTime? ReadByClientDate { get; set; }
    public DateTime? ReadBySpecialistDate { get; set; }
}
