namespace KV.Server.Permissions.TicketSections;

public class TicketSectionPermissions
{
    public const string GroupName = "TicketSection";

    public static class TicketSections
    {
        public const string Default = GroupName;

        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
