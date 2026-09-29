namespace KV.Server.Events;
using System;
using KV.Server.Tickets;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

/// <summary>
///     Событие
/// </summary>
public class CalendarEvent : FullAuditedEntityWithUser<Guid, IdentityUser>, IMultiTenant, IHasExtraProperties
{
    /// <summary>
    ///     Название события (госзакупки и тд)
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Описание события
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Тип события (None, Warning)
    /// </summary>
    public EventType EventType { get; set; } = EventType.None;

    /// <summary>
    ///     Дата события
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    ///     Заявка для события
    /// </summary>
    public long? TicketId { get; set; }

    public Ticket Ticket { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; protected set; }

    /// <summary>
    ///     Компания, клиента
    /// </summary>
    public Guid? TenantId { get; set; }
}
