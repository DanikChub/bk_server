namespace KV.Server.Contracts;
using System;
using KV.Server.TicketHoursSpent;
using Volo.Abp.Domain.Entities.Auditing;

/// <summary>
///     Списание часов по месяцам
/// </summary>
public class ContractSetting : AuditedEntity<Guid>
{
    public ContractSetting(int max, Guid constraintTypeId, Guid contractId)
    {
        this.Max = max;
        this.ConstraintTypeId = constraintTypeId;
        this.ContractId = contractId;
    }

    /// <summary>
    ///     Максимальное значение
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    ///     Ограничение (НПА и тд)
    /// </summary>
    public Guid ConstraintTypeId { get; set; }

    public ConstraintType ConstraintType { get; set; }

    /// <summary>
    ///     Контракт
    /// </summary>
    public Guid ContractId { get; set; }

    public Contract Contract { get; set; }
}
