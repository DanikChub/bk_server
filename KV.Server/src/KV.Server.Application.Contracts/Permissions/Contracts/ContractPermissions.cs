namespace KV.Server.Permissions.Contracts;

public class ContractPermissions
{
    public const string GroupName = "Contract";

    public static class Contracts
    {
        public const string Default = GroupName;
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
