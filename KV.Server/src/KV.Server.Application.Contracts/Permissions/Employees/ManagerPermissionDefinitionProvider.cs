namespace KV.Server.Permissions;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

public class ManagerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(ManagerPermissions.GroupName, L("Permission:ManagerManagement"));

        var ProfileCRUDPermission =
            ProfileGroup.AddPermission(ManagerPermissions.Profiles.Default, L("Permission:Profiles"));

        ProfileCRUDPermission.AddChild(ManagerPermissions.Profiles.Create, L("Permission:Profiles.Create"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(ManagerPermissions.Profiles.Edit, L("Permission:Profiles.Edit"));
        ProfileCRUDPermission.AddChild(ManagerPermissions.Profiles.Delete, L("Permission:Profiles.Delete"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(ManagerPermissions.Profiles.Get, L("Permission:Profiles.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
