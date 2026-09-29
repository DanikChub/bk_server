namespace KV.Server.Migration.Models;

using System.Collections.Generic;

public partial class ConsultingLibPostType
{
    public ConsultingLibPostType() => this.ConsultingLibraryPosts = new HashSet<ConsultingLibraryPost>();

    public int Id { get; set; }
    public string DisplayName { get; set; }
    public string IconUrl { get; set; }

    public virtual ICollection<ConsultingLibraryPost> ConsultingLibraryPosts { get; set; }
}
