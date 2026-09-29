namespace KV.Server.Migration.Models;

public partial class ApplicationUserTicketWatcher
{
    public int Id { get; set; }
    public int? TicketId { get; set; }
    public string UserId { get; set; }

    public virtual Hdticket Ticket { get; set; }
    public virtual AspNetUser User { get; set; }
}
