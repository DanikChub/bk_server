namespace KV.Server.Permissions;
using KV.Server.Localization;
using KV.Server.Permissions.Calendars;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

public class CalendarPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var ProfileGroup = context.AddGroup(CalendarPermissions.GroupName, L("Permission:CalendarManagement"));

        var CalendarCRUDPermission =
            ProfileGroup.AddPermission(CalendarPermissions.Calendars.Default, L("Permission:Calendar"));

        CalendarCRUDPermission.AddChild(CalendarPermissions.Calendars.Create, L("Permission:Calendar.Create"));
        CalendarCRUDPermission.AddChild(CalendarPermissions.Calendars.Edit, L("Permission:Calendar.Edit"));
        CalendarCRUDPermission.AddChild(CalendarPermissions.Calendars.Delete, L("Permission:Calendar.Delete"));
        CalendarCRUDPermission.AddChild(CalendarPermissions.Calendars.Get, L("Permission:Calendar.Get"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<ServerResource>(name);
}
