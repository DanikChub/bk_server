namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Themes.KvInternal.Components.Toolbar;

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
        return this.View("~/Themes/KvInternal/Components/Toolbar/Default.cshtml", toolbar);
    }
}
