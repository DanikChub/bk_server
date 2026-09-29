namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Раздел для заявки (муниципальный контроль и тд)
/// </summary>
public class TicketSection : Entity<Guid>
{
    public TicketSection(Guid id, string name)
    {
        this.Id = id;
        this.Name = name;
    }

    public TicketSection()
    {
    }

    /// <summary>
    ///     Название раздела
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Заявки
    /// </summary>
    public ICollection<Ticket> Tickets { get; set; }
}
