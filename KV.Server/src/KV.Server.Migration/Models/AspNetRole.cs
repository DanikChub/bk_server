namespace KV.Server.Migration.Models;

using System.Collections.Generic;

public partial class AspNetRole
{
    public AspNetRole()
    {
        this.AspNetRoleClaims = new HashSet<AspNetRoleClaim>();
        this.Users = new HashSet<AspNetUser>();
    }

    public string Id { get; set; }
    public string ConcurrencyStamp { get; set; }
    public string Name { get; set; }
    public string NormalizedName { get; set; }

    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual ICollection<AspNetUser> Users { get; set; }
}
