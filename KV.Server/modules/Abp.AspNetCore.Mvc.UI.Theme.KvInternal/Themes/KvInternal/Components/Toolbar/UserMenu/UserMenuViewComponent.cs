namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Themes.KvInternal.Components.Toolbar.UserMenu;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.UI.Navigation;

public class UserMenuViewComponent : AbpViewComponent
{
    private readonly IMenuManager _menuManager;

    public UserMenuViewComponent(IMenuManager menuManager)
    {
        this._menuManager = menuManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var menu = await this._menuManager.GetAsync(StandardMenus.User);
        return this.View("~/Themes/KvInternal/Components/Toolbar/UserMenu/Default.cshtml", menu);
    }
}
