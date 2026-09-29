namespace KV.Server.Migration.Extensions;

using System;
using KV.Server.Tickets;

public static class TicketStatusExtensions
{
    public static void SetId(this TicketStatus ticketStatus, Guid Id)
    {
        var info = typeof(TicketStatus).GetProperty("Id");
        info?.SetValue(ticketStatus, Id);
    }
}
