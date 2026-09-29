namespace KV.Server.Migration.Models;

using System;

public partial class DirectMessage
{
    public Guid Id { get; set; }
    public string AuthorId { get; set; }
    public DateTime Created { get; set; }
    public string Message { get; set; }
    public string RecipientId { get; set; }
    public DateTime? Seen { get; set; }
    public DateTime? Shown { get; set; }
    public string Title { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual AspNetUser Recipient { get; set; }
}
