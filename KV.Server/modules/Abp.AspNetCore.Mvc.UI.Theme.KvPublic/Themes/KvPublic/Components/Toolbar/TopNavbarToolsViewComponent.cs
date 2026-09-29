namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Themes.KvPublic.Components.Toolbar;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;

public class TopNavbarToolsViewComponent : AbpViewComponent
{
    private readonly IToolbarManager _toolbarManager;

    public TopNavbarToolsViewComponent(IToolbarManager toolbarManager)
    {
        this._toolbarManager = toolbarManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var toolbar = await this._toolbarManager.GetAsync(StandardToolbars.Main);
        return this.View("~/Themes/KvPublic/Components/Toolbar/Default.cshtml", toolbar);
    }
}
