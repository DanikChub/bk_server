namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class ClassifierOkei
{
    public ClassifierOkei() => this.InverseParent = new HashSet<ClassifierOkei>();

    public string Code { get; set; }
    public string Category { get; set; }
    public string DisplayCode { get; set; }
    public string DisplayName { get; set; }
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public bool? LeafNode { get; set; }

    public virtual ClassifierOkei Parent { get; set; }
    public virtual ICollection<ClassifierOkei> InverseParent { get; set; }
}
