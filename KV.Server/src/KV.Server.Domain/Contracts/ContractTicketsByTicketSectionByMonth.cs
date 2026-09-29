namespace KV.Server;
using System;
using KV.Server.Tickets;
using Volo.Abp.Domain.Entities;

public class ContractTicketsByTicketSectionByMonth : Entity<long>
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }

    /// <summary>
    ///     Контракт
    /// </summary>
    public Guid ContractId { get; set; }

    public Contract Contract { get; set; }

    /// <summary>
    ///     Тип заявки
    /// </summary>
    public Guid TicketSectionId { get; set; }

    public TicketSection TicketSection { get; set; }
}
