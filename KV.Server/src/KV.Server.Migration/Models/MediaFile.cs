namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class MediaFile
{
    public MediaFile()
    {
        this.CustomerMediaLibraryMedia = new HashSet<CustomerMediaLibraryMedia>();
        this.HdticketAttachments = new HashSet<HdticketAttachment>();
        this.SliderItems = new HashSet<SliderItem>();
    }

    public int Id { get; set; }
    public string AuthorId { get; set; }
    public DateTime Created { get; set; }
    public string DisplayName { get; set; }
    public string FileName { get; set; }
    public string MediaType { get; set; }
    public long Size { get; set; }
    public int? HdcomentId { get; set; }
    public int? HdticketStatusHistoryItemId { get; set; }
    public string OriginalFileName { get; set; }
    public int? HdticketStatusHistoryItemId1 { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual Hdcoment Hdcoment { get; set; }
    public virtual HdticketStatusHistory HdticketStatusHistoryItem { get; set; }
    public virtual HdticketStatusHistory HdticketStatusHistoryItemId1Navigation { get; set; }
    public virtual ICollection<CustomerMediaLibraryMedia> CustomerMediaLibraryMedia { get; set; }
    public virtual ICollection<HdticketAttachment> HdticketAttachments { get; set; }
    public virtual ICollection<SliderItem> SliderItems { get; set; }
}
