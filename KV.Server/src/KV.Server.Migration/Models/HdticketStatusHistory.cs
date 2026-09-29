namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class HdticketStatusHistory
{
    public HdticketStatusHistory() => this.MediaFileHdticketStatusHistoryItems = new HashSet<MediaFile>();

    public int Id { get; set; }
    public string AuthorId { get; set; }
    public string Comment { get; set; }
    public DateTime Created { get; set; }
    public bool IsPublic { get; set; }
    public int? StatusId { get; set; }
    public int? TicketId { get; set; }
    public bool IsCustomerMessage { get; set; }
    public bool ReadByClient { get; set; }
    public DateTime? ReadByClientDate { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual HdticketStatus Status { get; set; }
    public virtual Hdticket Ticket { get; set; }
    public virtual MediaFile MediaFileHdticketStatusHistoryItemId1Navigation { get; set; }
    public virtual ICollection<MediaFile> MediaFileHdticketStatusHistoryItems { get; set; }
}
