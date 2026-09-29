namespace KV.Server.Permissions;
using KV.Server.Localization;
using KV.Server.Permissions.TicketSections;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class TicketSectionPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup =
            context.AddGroup(TicketSectionPermissions.GroupName, L("Permission:TicketSectionManagement"));

        var TicketSectionCRUDPermission = ProfileGroup.AddPermission(TicketSectionPermissions.TicketSections.Default,
            L("Permission:TicketSection"));

        TicketSectionCRUDPermission.AddChild(TicketSectionPermissions.TicketSections.Create,
            L("Permission:TicketSection.Create"));
        TicketSectionCRUDPermission.AddChild(TicketSectionPermissions.TicketSections.Edit,
            L("Permission:TicketSection.Edit"));
        TicketSectionCRUDPermission.AddChild(TicketSectionPermissions.TicketSections.Delete,
            L("Permission:TicketSection.Delete"));
        TicketSectionCRUDPermission.AddChild(TicketSectionPermissions.TicketSections.Get,
            L("Permission:TicketSection.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
