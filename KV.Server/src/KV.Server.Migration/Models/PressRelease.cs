namespace KV.Server.Migration.Models;

using System;

public partial class PressRelease
{
    public int Id { get; set; }
    public string AuthorId { get; set; }
    public string Body { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Published { get; set; }
    public string ShortDescription { get; set; }
    public string Title { get; set; }
    public DateTime Updated { get; set; }
    public int? RegionId { get; set; }
    public string Tags { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual Region Region { get; set; }
}
