namespace KV.Server.Migration.Models;

using System;

public partial class UserEventVendor
{
    public long Id { get; set; }
    public Guid EventId { get; set; }
    public int VendorId { get; set; }

    public virtual UserEvent Event { get; set; }
    public virtual Vendor Vendor { get; set; }
}
