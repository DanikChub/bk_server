namespace KV.Server.Migration.Models;

using System;

public partial class ArchivedTicket
{
    public string AuthorFullName { get; set; }
    public string AuthorUserName { get; set; }
    public string AssignedToFullName { get; set; }
    public string AssignedToUserName { get; set; }
    public int VendorId { get; set; }
    public string VendorShortName { get; set; }
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastChanged { get; set; }
    public string ShortDescription { get; set; }
    public double Rating { get; set; }
    public int StatusId { get; set; }
    public string StatusDisplayName { get; set; }
    public string StatusName { get; set; }
}
