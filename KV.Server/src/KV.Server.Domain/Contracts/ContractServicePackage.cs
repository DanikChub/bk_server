namespace KV.Server.Contracts;
using System;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Контракты и Услуги
/// </summary>
public class ContractServicePackage : Entity
{
    /// <summary>
    ///     Ссылка на нужный контракт
    /// </summary>
    public Guid ContractId { get; set; }

    public Contract Contract { get; set; }

    /// <summary>
    ///     Ссылка на нужную услугу
    /// </summary>
    public Guid ServicePackageId { get; set; }

    public ServicePackage ServicePackage { get; set; }

    /// <summary>
    ///     Контракт начало действия
    /// </summary>
    public DateTime ContractStartDate { get; set; }

    /// <summary>
    ///     Контракт конец действия
    /// </summary>
    public DateTime ContractFinishDate { get; set; }

    public override object[] GetKeys() => new object[] { this.ContractId, this.ServicePackageId };
}
