namespace KV.Server;
using System;
using Volo.Abp.Data;

public class CreateEventInTicketDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public EventType EventType { get; set; } = EventType.None;
    public Guid? TenantId { get; set; }
    public DateTime EventDate { get; set; }
    public long? TicketId { get; set; }
    public ExtraPropertyDictionary ExtraProperties { get; set; }
}
