namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class ConsultingLibraryPost
{
    public ConsultingLibraryPost()
    {
        this.ConsultingLibraryCategories = new HashSet<ConsultingLibraryCategory>();
        this.Parents = new HashSet<ConsultingLibraryPost>();
        this.Relateds = new HashSet<ConsultingLibraryPost>();
    }

    public int Id { get; set; }
    public string AuthorId { get; set; }
    public string Body { get; set; }
    public DateTime Created { get; set; }
    public string FileThumbUrl { get; set; }
    public DateTime? Published { get; set; }
    public string ShortDescription { get; set; }
    public string Tags { get; set; }
    public string Title { get; set; }
    public DateTime Updated { get; set; }
    public int? CategoryId { get; set; }
    public bool? Deleted { get; set; }
    public string EditorId { get; set; }
    public int? PostTypeId { get; set; }
    public bool IsPublished { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual ConsultingLibraryCategory Category { get; set; }
    public virtual AspNetUser Editor { get; set; }
    public virtual ConsultingLibPostType PostType { get; set; }

    public virtual ICollection<ConsultingLibraryCategory> ConsultingLibraryCategories { get; set; }
    public virtual ICollection<ConsultingLibraryPost> Parents { get; set; }
    public virtual ICollection<ConsultingLibraryPost> Relateds { get; set; }
}
