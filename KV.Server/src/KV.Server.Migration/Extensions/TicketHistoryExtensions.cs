namespace KV.Server.Migration.Extensions;

using System;
using KV.Server.Tickets;

public static class TicketHistoryExtensions
{
    public static void SetCreationTime(this TicketHistory ticketHistory, DateTime CreationTime)
    {
        var info = typeof(TicketHistory).GetProperty("CreationTime");
        info?.SetValue(ticketHistory, CreationTime);
    }

    public static void SetLastModificationTime(this TicketHistory ticketHistory, DateTime LastModificationTime)
    {
        var info = typeof(TicketHistory).GetProperty("LastModificationTime");
        info?.SetValue(ticketHistory, LastModificationTime);
    }

    public static void SetCreatorId(this TicketHistory ticketHistory, Guid? CreatorId)
    {
        var info = typeof(TicketHistory).GetProperty("CreatorId");
        info?.SetValue(ticketHistory, CreatorId);
    }
}
