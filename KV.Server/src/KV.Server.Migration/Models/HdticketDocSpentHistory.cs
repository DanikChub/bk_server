namespace KV.Server.Migration.Models;

using System;

public partial class HdticketDocSpentHistory
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public DateTime Created { get; set; }
    public string AuthorId { get; set; }
    public string Comment { get; set; }
    public int SpentCount { get; set; }

    public virtual AspNetUser Author { get; set; }
    public virtual Hdticket Ticket { get; set; }
}
