namespace KV.Server.Migration.Models;

using System;

public partial class ClassifierOkpd
{
    public string Code { get; set; }
    public string Category { get; set; }
    public string DisplayCode { get; set; }
    public string DisplayName { get; set; }
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public bool? LeafNode { get; set; }
}
