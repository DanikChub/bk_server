namespace KV.Server.Permissions.Customers;

public static class CustomerUserPermissions
{
    public const string GroupName = "EmployeeManagement";

    public static class CustomerUsers
    {
        public const string Default = GroupName;
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
