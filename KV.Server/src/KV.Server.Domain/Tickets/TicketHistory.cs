namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using KV.Server.TicketHoursSpent;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

/// <summary>
///     История заявки
/// </summary>
public class TicketHistory : FullAuditedEntity<Guid>
{
    public TicketHistory(Guid? creatorId, string description, long ticketId, Guid ticketStatusId,
        TicketHistoryType type)
    {
        this.CreatorId = creatorId;
        this.Description = description;
        this.TicketId = ticketId;
        this.TicketStatusId = ticketStatusId;
        this.Type = type;
    }

    public void SetDescription(string description) => this.Description = description;
    public void SetInternal(bool value) => this.IsInternal = value;

    public bool IsInternal { get; protected set; }

    /// <summary>
    ///     Описание истории заявки
    /// </summary>
    public string Description { get; protected set; }

    /// <summary>
    ///     Прочитал специалист (с датой)
    /// </summary>
    public DateTime? ReadBySpecialistDate { get; set; }

    /// <summary>
    ///     Прочитал клиент (с датой)
    /// </summary>
    public DateTime? ReadByClientDate { get; set; }

    /// <summary>
    ///     Тип истории (нужен для отображения в вебе)
    /// </summary>
    public TicketHistoryType Type { get; set; }

    /// <summary>
    ///     Автор заявки
    /// </summary>
    public IdentityUser Creator { get; protected set; }

    /// <summary>
    ///     Ссылка на нужную заявку
    /// </summary>
    public long TicketId { get; protected set; }

    public Ticket Ticket { get; protected set; }

    /// <summary>
    ///     Статус для ответа
    /// </summary>
    public Guid TicketStatusId { get; protected set; }

    public TicketStatus TicketStatus { get; protected set; }

    /// <summary>
    ///     Вложеные заявки
    /// </summary>
    public ICollection<TicketHistoryAttachment> TicketHistoryAttachments { get; set; }

    /// <summary>
    ///     Если сообщение про списание
    /// </summary>
    public Guid? TicketHoursSpentHistoryId { get; set; }

    public void SetType(TicketHistoryType type)
    {
        Type = type;
    }

    public TicketHoursSpentHistory TicketHoursSpentHistory { get; set; }

    public void WriteDescription(string description) => this.Description = description;
}
