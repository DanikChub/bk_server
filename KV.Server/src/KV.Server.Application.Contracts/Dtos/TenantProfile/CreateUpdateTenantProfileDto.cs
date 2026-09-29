namespace KV.Server;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

/// <summary>
///     Data Transfer Object для TenantProfil для обмена данными между презентационным уровнем и сервисами.
/// </summary>
public class CreateUpdateTenantProfileDto : EntityDto<Guid>
{
    /// <summary>
    ///     Краткое наименование тенанта.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string ShortName { get; set; }

    /// <summary>
    ///     Полное наименование тенанта.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string LongName { get; set; }

    /// <summary>
    ///     Адрес тенанта.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Address { get; set; }
}
