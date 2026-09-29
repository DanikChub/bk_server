namespace KV.Server.Migration.Models;

public partial class VendorDocSpentHistory
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int SpentCount { get; set; }
    public int VendorId { get; set; }

    public virtual Vendor Vendor { get; set; }
}
