namespace KV.Server.Migration.Models;

using System;

public partial class UserNotification
{
    public int Id { get; set; }
    public string EventType { get; set; }
    public DateTime? PushSent { get; set; }
    public DateTime? Seen { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string UserId { get; set; }
    public DateTime Created { get; set; }

    public virtual AspNetUser User { get; set; }
}
