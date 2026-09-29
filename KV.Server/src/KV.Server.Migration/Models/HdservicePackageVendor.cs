namespace KV.Server.Migration.Models;

using System;

public partial class HdservicePackageVendor
{
    public int ServicePackageId { get; set; }
    public int VendorId { get; set; }
    public DateTime ContractEnd { get; set; }
    public string ContractNotes { get; set; }
    public string ContractNumber { get; set; }
    public DateTime ContractStart { get; set; }
    public bool IsActive { get; set; }
    public int Id { get; set; }
    public DateTime? ContractCreated { get; set; }
    public bool NotSigned { get; set; }

    public virtual HdservicePackage ServicePackage { get; set; }
    public virtual Vendor Vendor { get; set; }
}
