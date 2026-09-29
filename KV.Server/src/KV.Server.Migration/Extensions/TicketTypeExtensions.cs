namespace KV.Server.Migration.Extensions;

using System;
using KV.Server.Tickets;

public static class TicketTypeExtensions
{
    public static void SetId(this TicketType ticketType, Guid Id)
    {
        var info = typeof(TicketType).GetProperty("Id");
        info?.SetValue(ticketType, Id);
    }
}
