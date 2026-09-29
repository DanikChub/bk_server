namespace KV.Server.Permissions.Stories;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

public class StoriesPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(StoriesPermissions.GroupName, L("Permission:StoriesManagement"));

        var ProfileCRUDPermission = ProfileGroup.AddPermission(StoriesPermissions.Stories.Default,
            L("Permission:Stories"), MultiTenancySides.Host);

        ProfileCRUDPermission.AddChild(StoriesPermissions.Stories.Create, L("Permission:Stories.Create"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(StoriesPermissions.Stories.Edit, L("Permission:Stories.Edit"));
        ProfileCRUDPermission.AddChild(StoriesPermissions.Stories.Delete, L("Permission:Stories.Delete"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(StoriesPermissions.Stories.Get, L("Permission:Stories.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
