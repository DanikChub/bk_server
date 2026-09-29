namespace KV.Server;
using System;

public class CreateUpdateTicketMessageDto
{
    public string Comment { get; set; }
    public DateTime CreationTime { get; set; }
    public long TicketId { get; set; }
    public Guid CreatorCustomerUserProfileId { get; set; }
}
