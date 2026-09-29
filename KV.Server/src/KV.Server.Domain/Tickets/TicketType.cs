namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Тип заявки
/// </summary>
public class TicketType : Entity<Guid>
{
    public TicketType(Guid id, string name, TimeSpan slaTime)
    {
        this.Id = id;
        this.Name = name;
        this.SLATime = slaTime;
    }

    public TicketType()
    {
    }

    /// <summary>
    ///     Имя типа заявки
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    ///     Время на решение заявки
    /// </summary>
    public TimeSpan SLATime { get; set; }

    public ICollection<Ticket> Tickets { get; set; }
}
