namespace KV.Server.Permissions;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

public class EmployeeProfilePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(EmployeeProfilePermissions.GroupName, L("Permission:EmployeeManagement"));

        var ProfileCRUDPermission =
            ProfileGroup.AddPermission(EmployeeProfilePermissions.Profiles.Default, L("Permission:Profiles"));

        ProfileCRUDPermission.AddChild(EmployeeProfilePermissions.Profiles.Create, L("Permission:Profiles.Create"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(EmployeeProfilePermissions.Profiles.Edit, L("Permission:Profiles.Edit"));
        ProfileCRUDPermission.AddChild(EmployeeProfilePermissions.Profiles.Delete, L("Permission:Profiles.Delete"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(EmployeeProfilePermissions.Profiles.Get, L("Permission:Profiles.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
