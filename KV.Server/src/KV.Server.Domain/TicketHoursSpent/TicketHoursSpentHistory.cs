namespace KV.Server.TicketHoursSpent;
using System;
using KV.Server.Tickets;
using Volo.Abp.Domain.Entities.Auditing;

/// <summary>
///     Отдельная запись для истории потраченных часов
/// </summary>
public class TicketHoursSpentHistory : AuditedEntity<Guid>
{
    public TicketHoursSpentHistory(int spent, Guid constraintTypeId, Guid ticketHistoryId)
    {
        this.Spent = spent;
        this.ConstraintTypeId = constraintTypeId;
        this.TicketHistoryId = ticketHistoryId;
    }

    /// <summary>
    ///     Потрачено времени
    /// </summary>
    public virtual int Spent { get; protected set; }

    /// <summary>
    ///     Ограничение
    /// </summary>
    public virtual Guid ConstraintTypeId { get; protected set; }

    public virtual ConstraintType ConstraintType { get; protected set; }

    /// <summary>
    ///     Сообщение в история
    /// </summary>
    public virtual Guid TicketHistoryId { get; protected set; }

    public virtual TicketHistory TicketHistory { get; protected set; }
}
