namespace KV.Server.Web.Menus;
using System.Threading.Tasks;
using KV.Server.Localization;
using KV.Server.MultiTenancy;
using KV.Server.Permissions;
using KV.Server.Permissions.Contracts;
using KV.Server.Permissions.Stories;
using KV.Server.Permissions.Tickets;
using KV.Server.Permissions.TicketSections;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

public class ServerMenuContributor : IMenuContributor
{
#pragma warning disable CA1707
    public const string CUSTOMERS = "Customers";
    public const string TICKETS = "Tickets";
    public const string CONTRACTS = "Contracts";
    public const string USERS = "Users";
    public const string STORIES = "Stories";
    public const string CONTRACT_STATUSES = "ContractStatuses";
    public const string ANSWERTEMPLATE = "AnswerTemplates";
    public const string TICKET_SECTION = "TicketSections";
    public const string EMPLOYEE = "Employees";
#pragma warning restore CA1707

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

        context.Menu.AddItem(new ApplicationMenuItem(TICKETS, l["Ticket"], "/Tickets", "fas fa-envelope")
            .RequirePermissions(TicketPermissions.Tickets.Default));

        context.Menu.AddItem(new ApplicationMenuItem(CUSTOMERS, l["Customers"], "/Customers", "fas fa-users")
            .RequirePermissions(CustomerPermissions.Profiles.Default));

        context.Menu.AddItem(new ApplicationMenuItem(CONTRACTS, l["Contract"], "/Contracts", "far fa-file-alt")
            .RequirePermissions(ContractPermissions.Contracts.Default));

        context.Menu.AddItem(new ApplicationMenuItem(USERS, l["Users"], "/Users", "fas fa-user")
            .RequirePermissions(CustomerPermissions.Profiles.Default));

        // Tenant should not display
        //context.Menu.AddItem(new ApplicationMenuItem("Tenants", l["TenantProfile"], url: "/Tenants", icon: "fas fa-building")
        //            .RequirePermissions(TenantProfilePermissions.Profiles.Default)); 

        context.Menu.AddItem(new ApplicationMenuItem(STORIES, l["Stories"], "/Stories", "fas fa-file")
            .RequirePermissions(StoriesPermissions.Stories.Default));

        context.Menu.AddItem(
     new ApplicationMenuItem("Administration", l["Administration"], icon: "fas fa-cogs")
      .AddItem(new ApplicationMenuItem(EMPLOYEE, l["Employees"], "/Employees"))
             .RequirePermissions(TicketSectionPermissions.TicketSections.Default)
         .AddItem(new ApplicationMenuItem("Handbooks", l["Handbooks"], icon: "fas fa-bars")
             .AddItem(new ApplicationMenuItem(CONTRACT_STATUSES, l["ContractStatus"], "/ContractStatuses"))
             .RequirePermissions(ContractStatusesPermissions.Statuses.Default)
             .AddItem(new ApplicationMenuItem(ANSWERTEMPLATE, l["AnswerTemplate"], "/AnswerTemplates"))
             .RequirePermissions(AnswerTemplatePermissions.AnswerTemplates.Default)
             .AddItem(new ApplicationMenuItem(TICKET_SECTION, l["TicketSection"], "/TicketSections"))
             .RequirePermissions(TicketSectionPermissions.TicketSections.Default)

         ).RequirePermissions(ContractStatusesPermissions.Statuses.Default)
 );

        context.Menu.AddItem(new ApplicationMenuItem("LibraryOfConsultation", l["LibraryOfConsultation"],
            "https://bk.kv34.ru/", "fas fa-book"));

        context.Menu.AddItem(new ApplicationMenuItem("About", l["About"], "/About", "fas fa-question-circle"));

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
#pragma warning disable CS0162 // Unreachable code detected
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
#pragma warning restore CS0162 // Unreachable code detected
        }

        //.RequirePermissions("Tenant_Profile_CRUD")
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);
    }
}
