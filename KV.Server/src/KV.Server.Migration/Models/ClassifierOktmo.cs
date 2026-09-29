namespace KV.Server.Migration.Models;

using System;

public partial class ClassifierOktmo
{
    public double? CodeValue { get; set; }
    public string Category { get; set; }
    public double? DisplayCodeValue { get; set; }
    public string DisplayName { get; set; }
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public bool? LeafNode { get; set; }
    public string Code { get; set; }
    public string DisplayCode { get; set; }
}
