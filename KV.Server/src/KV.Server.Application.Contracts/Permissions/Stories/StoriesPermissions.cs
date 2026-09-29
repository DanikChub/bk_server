namespace KV.Server.Permissions.Stories;

public class StoriesPermissions
{
    public const string GroupName = "StoriesManagement";

    public static class Stories
    {
        public const string Default = GroupName;
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
