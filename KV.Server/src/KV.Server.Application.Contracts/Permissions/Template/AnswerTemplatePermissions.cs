namespace KV.Server.Permissions;

public class AnswerTemplatePermissions
{
    public const string GroupName = "AnswerTemplate";

    public static class AnswerTemplates
    {
        public const string Default = GroupName;

        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Get = Default + ".Get";
    }
}
