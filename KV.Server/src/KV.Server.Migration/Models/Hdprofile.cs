namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class Hdprofile
{
    public Hdprofile() => this.Hdtickets = new HashSet<Hdticket>();

    public int Id { get; set; }
    public string JobPost { get; set; }
    public string UserId { get; set; }
    public int VendorId { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public virtual AspNetUser User { get; set; }
    public virtual Vendor Vendor { get; set; }
    public virtual ICollection<Hdticket> Hdtickets { get; set; }
}
