namespace KV.Server.Migration.Extensions;

using System;
using KV.Server.Tickets;

public static class TicketExtensions
{
    public static void SetCreationTime(this Ticket ticket, DateTime CreationTime) => ticket.CreationTime = CreationTime;

    public static void SetLastModificationTime(this Ticket ticket, DateTime UpdateTime) => ticket.LastModificationTime = UpdateTime;

    public static void SetDueDate(this Ticket ticket, DateTime DueDate)
    {
        var info = typeof(Ticket).GetProperty("DueDate");
        info?.SetValue(ticket, DueDate);
    }

    public static void SetTenantId(this Ticket ticket, Guid? TenantId)
    {
        var info = typeof(Ticket).GetProperty("TenantId");
        info?.SetValue(ticket, TenantId);
    }

    public static void SetAttachmentsCount(this Ticket ticket, int AttachmentsCount)
    {
        var info = typeof(Ticket).GetProperty("AttachmentsCount");
        info?.SetValue(ticket, AttachmentsCount);
    }
}
