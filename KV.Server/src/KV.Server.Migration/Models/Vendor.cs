namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class Vendor
{
    public Vendor()
    {
        this.Contracts = new HashSet<OldModels.Contract>();
        this.Hdprofiles = new HashSet<Hdprofile>();
        this.HdservicePackageVendors = new HashSet<HdservicePackageVendor>();
        this.UserEventVendors = new HashSet<UserEventVendor>();
        this.VendorDocSpentHistories = new HashSet<VendorDocSpentHistory>();
    }

    public int Id { get; set; }
    public string DisplayName { get; set; }
    public string Address { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhoneNumber { get; set; }
    public string Description { get; set; }
    public string DirectorEmail { get; set; }
    public string DirectorFullName { get; set; }
    public string DirectorPhone { get; set; }
    public string Innnumber { get; set; }
    public bool IsActive { get; set; }
    public string Kppnumber { get; set; }
    public string OfficialName { get; set; }
    public string ShortName { get; set; }
    public string SiteUrl { get; set; }
    public string CommentNotes { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public int? RegionId { get; set; }
    public string DirectorLogin { get; set; }
    public string ContractOwner { get; set; }

    public virtual Region Region { get; set; }
    public virtual ICollection<OldModels.Contract> Contracts { get; set; }
    public virtual ICollection<Hdprofile> Hdprofiles { get; set; }
    public virtual ICollection<HdservicePackageVendor> HdservicePackageVendors { get; set; }
    public virtual ICollection<UserEventVendor> UserEventVendors { get; set; }
    public virtual ICollection<VendorDocSpentHistory> VendorDocSpentHistories { get; set; }
}
