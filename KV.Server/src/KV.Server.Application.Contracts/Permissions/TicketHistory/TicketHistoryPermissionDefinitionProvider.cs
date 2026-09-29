namespace KV.Server.Permissions;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class TicketHistoryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup =
            context.AddGroup(TicketHistoryPermissions.GroupName, L("Permission:TicketHistoryManagement"));

        var TicketHistoryCRUDPermission = ProfileGroup.AddPermission(TicketHistoryPermissions.TicketHistory.Default,
            L("Permission:TicketHistory"));

        TicketHistoryCRUDPermission.AddChild(TicketHistoryPermissions.TicketHistory.Create,
            L("Permission:TicketHistory.Create"));
        TicketHistoryCRUDPermission.AddChild(TicketHistoryPermissions.TicketHistory.Edit,
            L("Permission:TicketHistory.Edit"));
        TicketHistoryCRUDPermission.AddChild(TicketHistoryPermissions.TicketHistory.Delete,
            L("Permission:TicketHistory.Delete"));
        TicketHistoryCRUDPermission.AddChild(TicketHistoryPermissions.TicketHistory.Get,
            L("Permission:TicketHistory.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
