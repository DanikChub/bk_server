namespace KV.Server.Permissions;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

public class CustomerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(CustomerPermissions.GroupName, L("Permission:CustomerManagement"));

        var ProfileCRUDPermission =
            ProfileGroup.AddPermission(CustomerPermissions.Profiles.Default, L("Permission:Profiles"));

        ProfileCRUDPermission.AddChild(CustomerPermissions.Profiles.Create, L("Permission:Profiles.Create"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(CustomerPermissions.Profiles.Edit, L("Permission:Profiles.Edit"));
        ProfileCRUDPermission.AddChild(CustomerPermissions.Profiles.Delete, L("Permission:Profiles.Delete"),
            MultiTenancySides.Host);
        ProfileCRUDPermission.AddChild(CustomerPermissions.Profiles.Get, L("Permission:Profiles.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
