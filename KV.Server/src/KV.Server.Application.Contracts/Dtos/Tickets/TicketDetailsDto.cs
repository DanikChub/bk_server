namespace KV.Server.Dtos.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

public class TicketDetailsDto : FullAuditedEntityDto<long>
{
    [MaxLength(256)] public string Code { get; set; }

    public string Subject { get; set; }
    public string Description { get; set; }
    public string CreatorEmail { get; set; }
    public string CreatorFirstName { get; set; }
    public string CreatorLastName { get; set; }
    public string CreatorMiddleName { get; set; }
    public string CreatorPhoneNumber { get; set; }
    public string CreatorUserName { get; set; }
    public string CreatorJobPost { get; set; }
    public string ResponsibleJobPost { get; set; }
    public string ResponsibleEmail { get; set; }
    public string ResponsibleFirstName { get; set; }
    public string ResponsibleLastName { get; set; }
    public string ResponsibleMiddleName { get; set; }
    public string ResponsiblePhoneNumber { get; set; }
    public string ResponsibleUserName { get; set; }
    public string ResponsibleFileNameAvatar { get; set; }
    public string ResponsibleAvatarId { get; set; }
    public string ClientCompanyShortName { get; set; }
    public int AttachmentsCount { get; set; }
    public double Rating { get; set; }
    public string TicketStatusDisplayName { get; set; }
    public string TicketStatusName { get; set; }
    public string TicketStatusStyle { get; set; }
    public string TicketTypeName { get; set; }
    public string TicketSection { get; set; }
    public Guid TicketTypeId { get; set; }
    public Guid? TicketSectionId { get; set; }
    public DateTime? ReadBySpecialistDate { get; set; }
    public DateTime? ReadByClientDate { get; set; }
    public DateTime DueDate { get; set; }
    public string ServicePackages { get; set; }
    public List<TagDto> Tags { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? ResponsibleId { get; set; }
    public Guid TicketStatusId { get; set; }
}
