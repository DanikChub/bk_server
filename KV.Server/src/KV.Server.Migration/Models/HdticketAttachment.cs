namespace KV.Server.Migration.Models;

using System;

public partial class HdticketAttachment
{
    public int Id { get; set; }
    public string AuthorId { get; set; }
    public DateTime Created { get; set; }
    public int MediaFileInfoId { get; set; }
    public int TicketId { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual MediaFile MediaFileInfo { get; set; }
    public virtual Hdticket Ticket { get; set; }
}
