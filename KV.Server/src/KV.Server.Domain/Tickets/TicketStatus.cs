namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Статус заявки
/// </summary>
public class TicketStatus : Entity<Guid>
{
    /// <summary>
    ///     Имя статуса заявки
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    ///     Является ли публичным
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    ///     Отображение имени
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     Стиль отображения в виде css
    /// </summary>
    public string Style { get; set; }

    /// <summary>
    ///     Отображение в виде множественного числа
    /// </summary>
    public string DisplayNameMany { get; set; }

    /// <summary>
    ///     Иконка
    /// </summary>
    public string Icon { get; set; }

    public ICollection<Ticket> Tickets { get; set; }
    public ICollection<TicketHistory> TicketHistories { get; set; }
}
