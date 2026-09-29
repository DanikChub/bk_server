namespace KV.Server.Permissions.Tickets;

public class TicketPermissions
{
    public const string GroupName = "Ticket";

    public static class Tickets
    {
        public const string Default = GroupName;

        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
