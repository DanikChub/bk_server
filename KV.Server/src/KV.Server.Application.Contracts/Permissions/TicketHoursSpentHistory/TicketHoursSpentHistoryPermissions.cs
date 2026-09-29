namespace KV.Server.Permissions;

public class TicketHoursSpentHistoryPermissions
{
    public const string GroupName = "TicketHoursSpentHistory";

    public static class TicketHoursSpentHistory
    {
        public const string Default = GroupName;

        public const string Create = Default + ".Create";
    }
}
