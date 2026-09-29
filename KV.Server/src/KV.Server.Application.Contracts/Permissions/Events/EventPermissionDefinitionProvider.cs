namespace KV.Server.Permissions;
using KV.Server.Localization;
using KV.Server.Permissions.Events;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class EventPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(EventPermissions.GroupName, L("Permission:EventManagement"));

        var EventCRUDPermission = ProfileGroup.AddPermission(EventPermissions.Events.Default, L("Permission:Event"));

        EventCRUDPermission.AddChild(EventPermissions.Events.Create, L("Permission:Event.Create"));
        EventCRUDPermission.AddChild(EventPermissions.Events.Edit, L("Permission:Event.Edit"));
        EventCRUDPermission.AddChild(EventPermissions.Events.Delete, L("Permission:Event.Delete"));
        EventCRUDPermission.AddChild(EventPermissions.Events.Get, L("Permission:Event.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
