namespace KV.Server.Permissions;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

public class TenantProfilePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup =
            context.AddGroup(TenantProfilePermissions.GroupName, L("Permission:TenantProfileManagement"));

        var ProfileCRUDPermission =
            ProfileGroup.AddPermission(TenantProfilePermissions.Profiles.Default, L("Permission:Profiles"));

        ProfileCRUDPermission.AddChild(TenantProfilePermissions.Profiles.Create, L("Permission:Profiles.Create"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(TenantProfilePermissions.Profiles.Edit, L("Permission:Profiles.Edit"));
        ProfileCRUDPermission.AddChild(TenantProfilePermissions.Profiles.Delete, L("Permission:Profiles.Delete"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(TenantProfilePermissions.Profiles.Get, L("Permission:Profiles.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
