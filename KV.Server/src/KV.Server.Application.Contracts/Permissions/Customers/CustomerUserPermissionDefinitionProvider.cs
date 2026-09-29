namespace KV.Server.Permissions.Customers;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

public class CustomerUserPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(CustomerUserPermissions.GroupName, L("Permission:CustomerUserManagement"));

        var ProfileCRUDPermission =
            ProfileGroup.AddPermission(CustomerUserPermissions.CustomerUsers.Default, L("Permission:CustomerUser"));

        ProfileCRUDPermission.AddChild(CustomerUserPermissions.CustomerUsers.Create, L("Permission:CustomerUser.Create"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(CustomerUserPermissions.CustomerUsers.Edit, L("Permission:CustomerUser.Edit"));
        ProfileCRUDPermission.AddChild(CustomerUserPermissions.CustomerUsers.Delete, L("Permission:CustomerUser.Delete"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(CustomerUserPermissions.CustomerUsers.Get, L("Permission:CustomerUser.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
