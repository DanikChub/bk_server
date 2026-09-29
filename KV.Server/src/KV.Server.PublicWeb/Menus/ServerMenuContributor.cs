namespace KV.Server.PublicWeb.Menus;
using KV.Server.Localization;
using KV.Server.MultiTenancy;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

public class ServerMenuContributor : IMenuContributor
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            ConfigureMainMenu(context);
        }
    }

    private static void ConfigureMainMenu(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<ServerResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                ServerMenus.Home,
                l["Menu:Home"],
                "~/",
                "fas fa-home",
                0
            )
        );

        context.Menu.AddItem(new ApplicationMenuItem("Tickets", l["Compose"], "/Tickets/Compose", "fas fa-pencil-alt"));
        context.Menu.AddItem(new ApplicationMenuItem("Tickets", l["Tickets"], "/Tickets", "fas fa-envelope"));
        context.Menu.AddItem(new ApplicationMenuItem("Calendar", l["Calendar"], "/Calendar", "fas fa-bars"));
        context.Menu.AddItem(new ApplicationMenuItem("Profile", l["Profile"], "/Profile", "fas fa-user"));
        context.Menu.AddItem(new ApplicationMenuItem("LibraryOfConsultation", l["LibraryOfConsultation"],
            "https://bk.kv34.ru/", "fas fa-book"));
        context.Menu.AddItem(new ApplicationMenuItem("About", l["About"], "/About", "fas fa-question-circle"));

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
            administration.TryRemoveMenuItem(IdentityMenuNames.GroupName);
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }
        else
        {
#pragma warning disable CS0162 // Unreachable code detected
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
            administration.TryRemoveMenuItem(IdentityMenuNames.GroupName);
            administration.TryRemoveMenuItem(SettingManagementMenuNames.GroupName);
#pragma warning restore CS0162 // Unreachable code detected
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);
    }
}
