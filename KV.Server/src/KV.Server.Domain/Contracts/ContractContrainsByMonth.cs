namespace KV.Server;
using System;
using KV.Server.TicketHoursSpent;
using Volo.Abp.Domain.Entities;

public class ContractConstraintsByMonth : Entity<long>
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int MaxCount { get; set; }
    public int Sum { get; set; }

    /// <summary>
    ///     Контракт
    /// </summary>
    public Guid ContractId { get; set; }

    public Contract Contract { get; set; }

    /// <summary>
    ///     Ограничение
    /// </summary>
    public Guid ConstraintTypeId { get; set; }

    public ConstraintType ConstraintType { get; set; }
}
