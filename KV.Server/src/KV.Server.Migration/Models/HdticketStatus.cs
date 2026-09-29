namespace KV.Server.Migration.Models;

using System.Collections.Generic;

public partial class HdticketStatus
{
    public HdticketStatus()
    {
        this.HdticketStatusHistories = new HashSet<HdticketStatusHistory>();
        this.Hdtickets = new HashSet<Hdticket>();
    }

    public int Id { get; set; }
    public string DisplayName { get; set; }
    public bool IsPublic { get; set; }
    public string Name { get; set; }

    public virtual ICollection<HdticketStatusHistory> HdticketStatusHistories { get; set; }
    public virtual ICollection<Hdticket> Hdtickets { get; set; }
}
