namespace KV.Server.Permissions.TicketMessages;

public class TicketMessagePermissions
{
    public const string GroupName = "TicketMessage";

    public static class TicketMessages
    {
        public const string Default = GroupName;
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
