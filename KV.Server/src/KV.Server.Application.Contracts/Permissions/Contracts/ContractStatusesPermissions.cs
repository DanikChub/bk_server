namespace KV.Server.Permissions.Contracts;

public class ContractStatusesPermissions
{
    public const string GroupName = "ContractStatus";

    public static class Statuses
    {
        public const string Default = GroupName;
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
