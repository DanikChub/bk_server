namespace KV.Server.Tickets;
using System;
using KV.Server.File;
using Volo.Abp.Domain.Entities;

public class TicketHistoryAttachment : Entity
{
    /// <summary>
    ///     Ссылка на нужный файл
    /// </summary>
    public Guid UploadedFileId { get; set; }

    public UploadedFile UploadedFile { get; set; }

    /// <summary>
    ///     Ссылка на нужную историю
    /// </summary>
    public Guid TicketHistoryId { get; set; }

    public TicketHistory TicketHistory { get; set; }

    public override object[] GetKeys() => new object[] { this.UploadedFileId, this.TicketHistoryId };
}
