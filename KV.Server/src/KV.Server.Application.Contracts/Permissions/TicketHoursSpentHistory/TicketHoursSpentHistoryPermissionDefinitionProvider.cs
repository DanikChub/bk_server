namespace KV.Server.Permissions;
using KV.Server.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class TicketHoursSpentHistoryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(TicketHoursSpentHistoryPermissions.GroupName,
            L("Permission:TicketHoursSpentHistoryManagement"));

        var TicketHoursSpentHistoryCRUDPermission = ProfileGroup.AddPermission(
            TicketHoursSpentHistoryPermissions.TicketHoursSpentHistory.Default,
            L("Permission:TicketHoursSpentHistory"));

        TicketHoursSpentHistoryCRUDPermission.AddChild(
            TicketHoursSpentHistoryPermissions.TicketHoursSpentHistory.Create,
            L("Permission:TicketHoursSpentHistory.Create"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
