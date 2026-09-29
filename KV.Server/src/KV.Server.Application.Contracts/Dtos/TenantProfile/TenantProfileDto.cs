namespace KV.Server;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

public class TenantProfileDto : EntityDto<Guid>
{
    [Required][MaxLength(1000)] public string ShortName { get; set; }

    [Required][MaxLength(1000)] public string LongName { get; set; }

    [Required][MaxLength(2000)] public string Address { get; set; }
}
