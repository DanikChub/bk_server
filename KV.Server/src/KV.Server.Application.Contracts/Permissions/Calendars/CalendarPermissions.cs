namespace KV.Server.Permissions.Calendars;

public class CalendarPermissions
{
    public const string GroupName = "Calendar";

    public static class Calendars
    {
        public const string Default = GroupName;

        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
