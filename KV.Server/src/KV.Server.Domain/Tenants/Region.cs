namespace KV.Server.Tenants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

public class Region : Entity<Guid>
{
    /// <summary>
    ///     Название региона
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    ///     Код статуса контракта
    /// </summary>
    [MaxLength(256)]
    public string Code { get; set; }

    public ICollection<TenantProfile> TenantsProfile { get; set; }
}
