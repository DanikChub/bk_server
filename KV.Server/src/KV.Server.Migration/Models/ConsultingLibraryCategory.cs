namespace KV.Server.Migration.Models;

using System.Collections.Generic;

public partial class ConsultingLibraryCategory
{
    public ConsultingLibraryCategory()
    {
        this.ConsultingLibraryPosts = new HashSet<ConsultingLibraryPost>();
        this.ConsultingLibraryPostsNavigation = new HashSet<ConsultingLibraryPost>();
    }

    public int Id { get; set; }
    public string Title { get; set; }

    public virtual ICollection<ConsultingLibraryPost> ConsultingLibraryPosts { get; set; }

    public virtual ICollection<ConsultingLibraryPost> ConsultingLibraryPostsNavigation { get; set; }
}
