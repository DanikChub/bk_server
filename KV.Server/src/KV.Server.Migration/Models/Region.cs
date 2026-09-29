namespace KV.Server.Migration.Models;

using System.Collections.Generic;

public partial class Region
{
    public Region()
    {
        this.PressReleases = new HashSet<PressRelease>();
        this.Vendors = new HashSet<Vendor>();
    }

    public int Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }

    public virtual ICollection<PressRelease> PressReleases { get; set; }
    public virtual ICollection<Vendor> Vendors { get; set; }
}
