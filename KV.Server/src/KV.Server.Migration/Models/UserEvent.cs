namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class UserEvent
{
    public UserEvent() => this.UserEventVendors = new HashSet<UserEventVendor>();

    public Guid Id { get; set; }
    public string AuthorId { get; set; }
    public DateTime Created { get; set; }
    public DateTime EventDateEnd { get; set; }
    public DateTime EventDateStart { get; set; }
    public string Message { get; set; }
    public string RecipientId { get; set; }
    public string Title { get; set; }
    public int RecipientCustomerId { get; set; }
    public bool RecipientIsCustomer { get; set; }
    public string Content { get; set; }
    public bool CustomerEvent { get; set; }
    public string RssTrackNumber { get; set; }
    public Guid TrackId { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual AspNetUser Recipient { get; set; }
    public virtual ICollection<UserEventVendor> UserEventVendors { get; set; }
}
