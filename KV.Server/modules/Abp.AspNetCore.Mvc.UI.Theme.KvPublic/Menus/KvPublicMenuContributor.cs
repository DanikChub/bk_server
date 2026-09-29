namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Menus;

using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;

public class KvPublicMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
