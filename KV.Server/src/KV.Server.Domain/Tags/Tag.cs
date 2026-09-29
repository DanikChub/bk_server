namespace KV.Server.Tags;
using System;
using System.Collections.Generic;
using KV.Server.Contracts;
using KV.Server.Tickets;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Теги (под заявкой)
/// </summary>
public class Tag : Entity<Guid>
{
    /// <summary>
    ///     Название тега
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Коллекция тегов под заявкой
    /// </summary>
    public virtual ICollection<TicketTag> TicketTags { get; protected set; }

    /// <summary>
    ///     Коллекция тегов под контрактом
    /// </summary>
    public virtual ICollection<ContractTag> ContractTags { get; protected set; }
}
