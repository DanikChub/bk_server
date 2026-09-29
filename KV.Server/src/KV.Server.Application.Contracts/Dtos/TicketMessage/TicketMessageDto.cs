namespace KV.Server;
using System;

public class TicketMessageDto
{
    public string Comment { get; set; }
    public DateTime CreationTime { get; set; }
    public long TicketId { get; set; }
    public Guid CreatorId { get; set; }
    public string CreatorFirstName { get; set; }
    public string CreatorLastName { get; set; }
    public string CreatorMiddleName { get; set; }
    public string CreatorUserName { get; set; }
    public string CreatorEmail { get; set; }
}
