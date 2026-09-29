namespace KV.Server;
using System;
using System.Collections.Generic;
using KV.Server.Contracts;
using KV.Server.Tickets;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

/// <summary>
///     Контракт
/// </summary>
public class Contract : FullAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>
    ///     Имя контракта
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Начало контракта
    /// </summary>
    public DateTime ContractStartDate { get; set; }

    /// <summary>
    ///     Конец контракта
    /// </summary>
    public DateTime ContractFinishDate { get; set; }

    /// <summary>
    ///     На кого заключён договор
    /// </summary>
    public string ContractPayer { get; set; }

    /// <summary>
    ///     Статус контракта
    /// </summary>
    public Guid StatusId { get; set; }

    public ContractStatus Status { get; set; }
    public virtual TenantProfile TenantProfile { get; set; }

    /// <summary>
    ///     находится в черновике
    /// </summary>
    public bool IsDraft { get; set; }

    /// <summary>
    ///     находится в архиве
    /// </summary>
    public bool IsArchive { get; set; }

    /// <summary>
    ///     Коллекция тегов под контрактом
    /// </summary>
    public ICollection<ContractTag> ContractTags { get; set; }

    /// <summary>
    ///     Коллекция услуг под контрактом
    /// </summary>
    public ICollection<ContractServicePackage> ServicePackages { get; set; }

    public virtual string ServicePackagesData { get; protected set; }
    /// <summary>
    ///     Коллекция заявок под контрактом
    /// </summary>
    public ICollection<Ticket> Tickets { get; set; }

    /// <summary>
    ///     Коллекция настройки контрактов
    /// </summary>
    public ICollection<ContractSetting> ContractSettings { get; set; }

    /// <summary>
    ///     Ссылка на тенанта
    /// </summary>
    public Guid? TenantId { get; set; }
}
