namespace KV.Server.Permissions;
using Volo.Abp.Authorization.Permissions;

public class ServerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ServerPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(ServerPermissions.MyPermission1, L("Permission:MyPermission1"));
    }
}
