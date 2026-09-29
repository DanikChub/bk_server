namespace KV.Server.Migration.Models;

using System.Collections.Generic;

public partial class HdservicePackage
{
    public HdservicePackage()
    {
        this.Contracts = new HashSet<OldModels.Contract>();
        this.HdservicePackageVendors = new HashSet<HdservicePackageVendor>();
        this.Hdtickets = new HashSet<Hdticket>();
    }

    public int Id { get; set; }
    public string Name { get; set; }

    public virtual ICollection<OldModels.Contract> Contracts { get; set; }
    public virtual ICollection<HdservicePackageVendor> HdservicePackageVendors { get; set; }

    public virtual ICollection<Hdticket> Hdtickets { get; set; }
}
