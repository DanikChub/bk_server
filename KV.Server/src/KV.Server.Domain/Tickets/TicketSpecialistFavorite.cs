namespace KV.Server.Tickets;
using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;

/// <summary>
///     Избранные заявки
/// </summary>
public class TicketSpecialistFavorite : Entity
{
    /// <summary>
    ///     Пользователь
    /// </summary>
    public Guid SpecialistId { get; set; }

    public IdentityUser Specialist { get; set; }

    /// <summary>
    ///     Заявка
    /// </summary>
    public long TicketId { get; set; }

    public Ticket Ticket { get; set; }

    public override object[] GetKeys() => new object[] { this.SpecialistId, this.TicketId };
}
