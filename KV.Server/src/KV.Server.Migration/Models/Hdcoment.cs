namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class Hdcoment
{
    public Hdcoment() => this.MediaFiles = new HashSet<MediaFile>();

    public int Id { get; set; }
    public string AuthorId { get; set; }
    public string Comment { get; set; }
    public DateTime Created { get; set; }
    public int EntityId { get; set; }
    public string EntityType { get; set; }
    public int? HdticketId { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual Hdticket Hdticket { get; set; }
    public virtual ICollection<MediaFile> MediaFiles { get; set; }
}
