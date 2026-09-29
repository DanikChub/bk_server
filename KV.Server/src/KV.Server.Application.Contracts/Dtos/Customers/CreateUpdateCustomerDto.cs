namespace KV.Server;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

public class CreateUpdateCustomerDto : EntityDto<Guid>
{
    [Required][MaxLength(1000)] public string ShortName { get; set; }

    [Required][MaxLength(2000)] public string LongName { get; set; }

    [MaxLength(2000)] public string Address { get; set; }

    public string SiteUrl { get; set; }
    public string Description { get; set; }
    public string INNNumber { get; set; }
    public string KPPNumber { get; set; }
    public string CommentNotes { get; set; }
    public string DisplayName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public Guid TenantId { get; set; }
    public Guid? RegionId { get; set; }
    public Guid? ResponsibleManagerId { get; set; }
    public Guid? CreatorId { get; set; }
}
