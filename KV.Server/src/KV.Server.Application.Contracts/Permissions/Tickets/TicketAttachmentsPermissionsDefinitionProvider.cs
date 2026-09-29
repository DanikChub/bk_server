namespace KV.Server.Permissions;
using KV.Server.Localization;
using KV.Server.Permissions.Tickets;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class TicketAttachmentsPermissionsDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(TicketAttachmentsPermissions.GroupName, L("Permission:TicketAttachmentsManagement"));

        var TicketCRUDPermission =
            ProfileGroup.AddPermission(TicketAttachmentsPermissions.TicketAttachments.Default, L("Permission:TicketAttachments"));

        TicketCRUDPermission.AddChild(TicketAttachmentsPermissions.TicketAttachments.Create, L("Permission:TicketAttachments.Create"));
        TicketCRUDPermission.AddChild(TicketAttachmentsPermissions.TicketAttachments.Edit, L("Permission:TicketAttachments.Edit"));
        TicketCRUDPermission.AddChild(TicketAttachmentsPermissions.TicketAttachments.Delete, L("Permission:TicketAttachments.Delete"));
        TicketCRUDPermission.AddChild(TicketAttachmentsPermissions.TicketAttachments.Get, L("Permission:TicketAttachments.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
