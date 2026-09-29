namespace KV.Server.Permissions;

public class TicketHistoryPermissions
{
    public const string GroupName = "TicketHistory";

    public static class TicketHistory
    {
        public const string Default = GroupName;

        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
        public const string Create = Default + ".Create";
    }
}
