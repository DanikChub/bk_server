namespace KV.Server.Tickets;
using System;
using KV.Server.File;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Файл для заявки
/// </summary>
public class TicketAttachment : Entity
{
    /// <summary>
    ///     Ссылка на нужный файл
    /// </summary>
    public Guid UploadedFileId { get; set; }

    public UploadedFile UploadedFile { get; set; }

    /// <summary>
    ///     Ссылка на нужную заявку
    /// </summary>
    public long TicketId { get; set; }

    public Ticket Ticket { get; set; }

    public override object[] GetKeys() => new object[] { this.UploadedFileId, this.TicketId };
}
