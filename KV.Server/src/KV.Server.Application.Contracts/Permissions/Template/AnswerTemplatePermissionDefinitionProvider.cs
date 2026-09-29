namespace KV.Server.Permissions;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class AnswerTemplatePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup =
            context.AddGroup(AnswerTemplatePermissions.GroupName, L("Permission:AnswerTemplateManagement"));

        var AnswerTemplateCRUDPermission = ProfileGroup.AddPermission(AnswerTemplatePermissions.AnswerTemplates.Default,
            L("Permission:AnswerTemplate"));

        AnswerTemplateCRUDPermission.AddChild(AnswerTemplatePermissions.AnswerTemplates.Create,
            L("Permission:AnswerTemplate.Create"));
        AnswerTemplateCRUDPermission.AddChild(AnswerTemplatePermissions.AnswerTemplates.Edit,
            L("Permission:AnswerTemplate.Edit"));
        AnswerTemplateCRUDPermission.AddChild(AnswerTemplatePermissions.AnswerTemplates.Delete,
            L("Permission:AnswerTemplate.Delete"));
        AnswerTemplateCRUDPermission.AddChild(AnswerTemplatePermissions.AnswerTemplates.Get,
            L("Permission:AnswerTemplate.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
