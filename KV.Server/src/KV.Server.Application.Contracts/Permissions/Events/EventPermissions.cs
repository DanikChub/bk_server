namespace KV.Server.Permissions.Events;

public class EventPermissions
{
    public const string GroupName = "Event";

    public static class Events
    {
        public const string Default = GroupName;

        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
