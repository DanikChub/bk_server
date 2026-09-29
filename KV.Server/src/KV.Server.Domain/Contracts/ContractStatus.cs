namespace KV.Server;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

/// <summary>
///     Статус контракта
/// </summary>
public class ContractStatus : Entity<Guid>, IMultiTenant
{
    /// <summary>
    ///     Заголовок состояния статуса контракта
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Код статуса контракта
    /// </summary>
    [MaxLength(256)]
    public string Code { get; set; }

    /// <summary>
    ///     Ссылка на тенант
    /// </summary>
    public Guid? TenantId { get; set; }

    public void SetStatusId(Guid statusId)
    {
        Id = statusId;
    }
}
