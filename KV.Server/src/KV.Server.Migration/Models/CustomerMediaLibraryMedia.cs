namespace KV.Server.Migration.Models;

using System;

public partial class CustomerMediaLibraryMedia
{
    public int Id { get; set; }
    public string AuthorId { get; set; }
    public DateTime Created { get; set; }
    public int? FileContentId { get; set; }
    public string FileThumbUrl { get; set; }
    public DateTime? Published { get; set; }
    public string Tags { get; set; }
    public string Title { get; set; }
    public DateTime Updated { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual MediaFile FileContent { get; set; }
}
