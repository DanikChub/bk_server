namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Themes.KvInternal.Components.Menu;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.UI.Navigation;

public class LeftNavbarMenuViewComponent : AbpViewComponent
{
    private readonly IMenuManager _menuManager;

    public LeftNavbarMenuViewComponent(IMenuManager menuManager)
    {
        this._menuManager = menuManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var menu = await this._menuManager.GetAsync(StandardMenus.Main);
        return this.View("~/Themes/KvInternal/Components/Menu/Default.cshtml", menu);
    }

    public static bool ChildActive(ApplicationMenuItemList menu, string currentName)
    {
        foreach (var item in menu)
        {
            if (item.Name == currentName)
            {
                return true;
            }

            if (ChildActive(item.Items, currentName))
            {
                return true;
            }
        }
        return false;
    }

    public static string ReWriteIcon(string menu, string icon)
    {
        if (icon.IsNullOrEmpty())
        {
            return "fas";
        }

        return menu switch
        {
            "AbpIdentity" => "fas fa-id-card",
            _ => icon.StartsWith("fa ", StringComparison.InvariantCultureIgnoreCase) ?
                icon.Replace("fa ", "fas ", StringComparison.InvariantCultureIgnoreCase) : icon,
        };
    }
}
