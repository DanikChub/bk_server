namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Menus;

using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;

public class KvInternalMenuContributor : IMenuContributor
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
