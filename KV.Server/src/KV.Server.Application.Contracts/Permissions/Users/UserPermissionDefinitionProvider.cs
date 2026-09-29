namespace KV.Server.Permissions;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

public class UserPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(UserPermissions.GroupName, L("Permission:UserManagement"));

        var ProfileCRUDPermission =
            ProfileGroup.AddPermission(UserPermissions.Profiles.Default, L("Permission:Profiles"));

        ProfileCRUDPermission.AddChild(UserPermissions.Profiles.Create, L("Permission:Profiles.Create"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(UserPermissions.Profiles.Edit, L("Permission:Profiles.Edit"));
        ProfileCRUDPermission.AddChild(UserPermissions.Profiles.Delete, L("Permission:Profiles.Delete"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(UserPermissions.Profiles.Get, L("Permission:Profiles.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
