namespace KV.Server.Migration.Models;

using System;

public partial class SliderItem
{
    public int Id { get; set; }
    public string AuthorId { get; set; }
    public DateTime Created { get; set; }
    public int? ImageId { get; set; }
    public int OrderIndex { get; set; }
    public DateTime? Published { get; set; }
    public string ShortDescription { get; set; }
    public string Title { get; set; }
    public DateTime Updated { get; set; }
    public string Url { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual MediaFile Image { get; set; }
}
