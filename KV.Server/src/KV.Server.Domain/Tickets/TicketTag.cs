namespace KV.Server.Tickets;
using System;
using KV.Server.Tags;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Теги под заявкой
/// </summary>
public class TicketTag : Entity
{
    /// <summary>
    ///     Заявка
    /// </summary>
    public long TicketId { get; set; }

    public Ticket Ticket { get; set; }

    /// <summary>
    ///     Тег
    /// </summary>
    public Guid TagId { get; set; }

    public Tag Tag { get; set; }

    public override object[] GetKeys() => new object[] { this.TicketId, this.TagId };
}
