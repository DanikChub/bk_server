namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using KV.Server.Events;
using KV.Server.Profiles;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

/// <summary>
///     Заявка
/// </summary>
public class Ticket : FullAuditedAggregateRoot<long>, IMultiTenant
{
    protected Ticket()
    {
    }

    public Ticket(long id,
        [NotNull] string subject,
        [NotNull] string description,
        Guid ticketTypeId,
        Guid ticketStatusId,
        Guid? responsibleId) : base(id)
    {
        this.Subject = subject;
        this.Description = description;
        this.TicketTypeId = ticketTypeId;
        this.TicketStatusId = ticketStatusId;
        this.ResponsibleId = responsibleId;

        this.TicketHistory = new Collection<TicketHistory>();
        this.TicketAttachments = new Collection<TicketAttachment>();
    }

    public Ticket(
        [NotNull] string subject,
        [NotNull] string description,
        Guid ticketTypeId,
        Guid ticketStatusId,
        Guid? responsibleId
    )
    {
        this.Subject = subject;
        this.Description = description;
        this.TicketTypeId = ticketTypeId;
        this.TicketStatusId = ticketStatusId;
        this.ResponsibleId = responsibleId;

        this.TicketHistory = new Collection<TicketHistory>();
        this.TicketAttachments = new Collection<TicketAttachment>();
    }

    /// <summary>
    ///     Отображение заголовка заявки
    /// </summary>
    [Required]
    public virtual string Subject { get; protected set; }

    /// <summary>
    ///     Описание заявки
    /// </summary>
    [Required]
    public virtual string Description { get; protected set; }

    /// <summary>
    ///     Количество вложеностей
    /// </summary>
    public virtual int AttachmentsCount { get; protected set; }

    /// <summary>
    ///     Рейтинг
    /// </summary>
    public virtual double Rating { get; protected set; }

    /// <summary>
    ///     Автор заявки
    /// </summary>
    public virtual IdentityUser Creator { get; protected set; }

    /// <summary>
    ///     Ответственное лицо заявки
    /// </summary>
    public virtual Guid? ResponsibleId { get; protected set; }

    public virtual EmployeeProfile Responsible { get; protected set; }

    /// <summary>
    ///     Тип заявки
    /// </summary>
    public virtual Guid TicketTypeId { get; protected set; }

    public virtual TicketType TicketType { get; protected set; }

    /// <summary>
    ///     Статус заявки
    /// </summary>
    public virtual Guid TicketStatusId { get; protected set; }

    public virtual TicketStatus TicketStatus { get; protected set; }

    /// <summary>
    ///     Раздел для заявки
    /// </summary>
    public virtual Guid? TicketSectionId { get; protected set; }

    public virtual TicketSection TicketSection { get; protected set; }

    /// <summary>
    ///     Коллекция файлов под заявкой
    /// </summary>
    public virtual ICollection<TicketAttachment> TicketAttachments { get; protected set; }

    /// <summary>
    ///     Коллекция ответов
    /// </summary>
    public virtual ICollection<TicketHistory> TicketHistory { get; protected set; }

    /// <summary>
    ///     Коллекция комментариев под заявкой
    /// </summary>
    public virtual ICollection<TicketMessage> TicketMessages { get; protected set; }

    /// <summary>
    ///     Коллекция тегов под заявкой
    /// </summary>
    public virtual ICollection<TicketTag> TicketTags { get; protected set; }

    public virtual TenantProfile TenantProfile { get; protected set; }

    /// <summary>
    ///     Контракт выбраный для заявки
    /// </summary>
    public virtual Guid? ContractId { get; set; }

    public virtual Contract Contract { get; set; }

    /// <summary>
    ///     Избранное
    /// </summary>
    public virtual ICollection<TicketClientFavorite> TicketClientFavorites { get; protected set; }

    public virtual ICollection<TicketSpecialistFavorite> TicketSpecialistFavorites { get; protected set; }
    public virtual ICollection<CalendarEvent> Events { get; protected set; }

    public virtual DateTime DueDate { get; protected set; } = DateTime.Now.AddDays(2.5);

    /// <summary>
    ///     Прочитал специалист (с датой)
    /// </summary>
    public DateTime? ReadBySpecialistDate { get; set; }

    /// <summary>
    ///     Прочитал клиент (с датой)
    /// </summary>
    public DateTime? ReadByClientDate { get; set; }

    /// <summary>
    ///     Ссылка на тенант
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }
    /// <summary>
    /// Пакеты услуг по связанному договору, разделённые запятой
    /// </summary>
    public virtual string ServicePackages { get; protected set; }
    public void SetTenant(Guid? tenantId)
    {
        this.TenantId = tenantId;
    }
    /// <summary>
    ///     Изменить статус заявки
    /// </summary>
    /// <param name="statusId"></param>
    /// <param name="comment"></param>
    public void UpdateStatus(Guid statusId, string comment, TicketHistoryType type)
    {
        if (statusId == this.TicketStatusId)
            return;

        this.TicketStatusId = statusId;
        var ticketHistory = new TicketHistory(null, comment, this.Id, statusId, type);
        this.TicketHistory.Add(ticketHistory);
    }

    /// <summary>
    ///     Установить конечное время
    /// </summary>
    /// <param name="deadline"></param>
    public void SetDeadLine(DateTime deadline) => this.DueDate = deadline;

    /// <summary>
    ///     Добавить вложеность
    /// </summary>
    /// <param name="ticketAttachment"></param>
    public void AddAttachment(TicketAttachment ticketAttachment)
    {
        this.TicketAttachments.Add(ticketAttachment);
        this.AttachmentsCount = this.TicketAttachments.Count;
    }

    /// <summary>
    ///     Удалить вложеность
    /// </summary>
    /// <param name="ticketAttachment"></param>
    public void RemoveAttachment(TicketAttachment ticketAttachment)
    {
        this.TicketAttachments.Remove(ticketAttachment);
        this.AttachmentsCount = this.TicketAttachments.Count;
    }

    /// <summary>
    ///     Назначить ответственное лицо
    /// </summary>
    /// <param name="userId"></param>
    public void AssignResponsible(Guid? customerUserProfileId) => this.ResponsibleId = customerUserProfileId;

    /// <summary>
    ///     Установить рейтинг
    /// </summary>
    /// <param name="newRating"></param>
    public void SetRating(double newRating) => this.Rating = newRating;

    /// <summary>
    ///     Обновить тип заявки
    /// </summary>
    /// <param name="ticketTypeId"></param>
    public void UpdateTicketType(Guid ticketTypeId) => this.TicketTypeId = ticketTypeId;

    public void UpdateTicketSection(Guid? ticketSectionId) => this.TicketSectionId = ticketSectionId;

    public void SetDescription(string description) => this.Description = description;
    public void SetDueDate(DateTime dueDate) => this.DueDate = dueDate;
}
