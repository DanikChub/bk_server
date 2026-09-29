namespace KV.Server.Permissions;

public static class CustomerPermissions
{
    public const string GroupName = "CustomerManagement";

    public static class Profiles
    {
        public const string Default = GroupName;
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
