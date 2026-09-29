namespace KV.Server.Migration.Models.OldModels;

using System;

public partial class Contract
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime EndDate { get; set; }
    public string Number { get; set; }
    public int ServicePackageId { get; set; }
    public DateTime StartDate { get; set; }
    public int VendorId { get; set; }

    public virtual HdservicePackage ServicePackage { get; set; }
    public virtual Vendor Vendor { get; set; }
}
