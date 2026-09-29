namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Themes.KvInternal.Components.Brand;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.UI.Navigation;

public class LeftNavbarBrandViewComponent : AbpViewComponent
{
    private readonly IMenuManager _menuManager;
    public LeftNavbarBrandViewComponent(IMenuManager menuManager)
    {
        this._menuManager = menuManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var menu = await this._menuManager.GetAsync(StandardMenus.User);
        return this.View("~/Themes/KvInternal/Components/Brand/Default.cshtml", menu);
    }
}
