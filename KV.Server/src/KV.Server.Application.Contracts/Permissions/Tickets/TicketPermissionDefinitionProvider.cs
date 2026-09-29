namespace KV.Server.Permissions;
using KV.Server.Localization;
using KV.Server.Permissions.Tickets;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class TicketPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(TicketPermissions.GroupName, L("Permission:TicketManagement"));

        var TicketCRUDPermission =
            ProfileGroup.AddPermission(TicketPermissions.Tickets.Default, L("Permission:Ticket"));

        TicketCRUDPermission.AddChild(TicketPermissions.Tickets.Create, L("Permission:Ticket.Create"));
        TicketCRUDPermission.AddChild(TicketPermissions.Tickets.Edit, L("Permission:Ticket.Edit"));
        TicketCRUDPermission.AddChild(TicketPermissions.Tickets.Delete, L("Permission:Ticket.Delete"));
        TicketCRUDPermission.AddChild(TicketPermissions.Tickets.Get, L("Permission:Ticket.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
