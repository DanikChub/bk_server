namespace KV.Server.Tickets;
using System;
using KV.Server.Profiles;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Комментарии под заявкой (внутренняя переписка)
/// </summary>
public class TicketMessage : Entity<Guid>
{
    /// <summary>
    ///     Содержание комментария
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     Время создание коментария
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    ///     Заявка
    /// </summary>
    public long TicketId { get; set; }

    public Ticket Ticket { get; set; }

    /// <summary>
    ///     Создатель
    /// </summary>
    public Guid CreatorCustomerUserProfileId { get; set; }

    public CustomerUserProfile CreatorCustomerUserProfile { get; set; }
}
