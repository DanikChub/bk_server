namespace KV.Server.Tickets;
using System;
using KV.Server.Profiles;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Избранные заявки
/// </summary>
public class TicketClientFavorite : Entity
{
    /// <summary>
    ///     Пользователь
    /// </summary>
    public Guid ClientId { get; set; }

    public CustomerUserProfile Client { get; set; }

    /// <summary>
    ///     Заявка
    /// </summary>
    public long TicketId { get; set; }

    public Ticket Ticket { get; set; }

    public override object[] GetKeys() => new object[] { this.ClientId, this.TicketId };
}
