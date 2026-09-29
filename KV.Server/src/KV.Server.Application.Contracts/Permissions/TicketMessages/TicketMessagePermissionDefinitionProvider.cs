namespace KV.Server.Permissions;
using KV.Server.Localization;
using KV.Server.Permissions.TicketMessages;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class TicketMessagePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup =
            context.AddGroup(TicketMessagePermissions.GroupName, L("Permission:TicketMessageManagement"));

        var TicketMessageCRUDPermission = ProfileGroup.AddPermission(TicketMessagePermissions.TicketMessages.Default,
            L("Permission:TicketMessage"));

        TicketMessageCRUDPermission.AddChild(TicketMessagePermissions.TicketMessages.Create,
            L("Permission:TicketMessage.Create"));
        TicketMessageCRUDPermission.AddChild(TicketMessagePermissions.TicketMessages.Edit,
            L("Permission:TicketMessage.Edit"));
        TicketMessageCRUDPermission.AddChild(TicketMessagePermissions.TicketMessages.Delete,
            L("Permission:TicketMessage.Delete"));
        TicketMessageCRUDPermission.AddChild(TicketMessagePermissions.TicketMessages.Get,
            L("Permission:TicketMessage.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
