namespace KV.Server.TicketHoursSpent;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Ограничение, НПА, Закупки, Торги, БУХУЧЕТ Претензии
/// </summary>
public class ConstraintType : Entity<Guid>
{
    public ConstraintType(Guid id, string name)
    {
        this.Id = id;
        this.Name = name;
    }

    /// <summary>
    ///     Название
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Коллекция списаний
    /// </summary>
    public ICollection<TicketHoursSpentHistory> TicketHoursSpentHistories { get; set; }
}
