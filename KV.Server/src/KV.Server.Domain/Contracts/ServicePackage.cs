namespace KV.Server.Contracts;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Услуга
/// </summary>
public class ServicePackage : Entity<Guid>
{
    /// <summary>
    ///     Имя услуги
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Стиль отображения в виде css
    /// </summary>
    public string Style { get; set; }

    /// <summary>
    ///     Коллекция услуг под контрактом
    /// </summary>
    public ICollection<ContractServicePackage> ServicePackegs { get; set; }
}
