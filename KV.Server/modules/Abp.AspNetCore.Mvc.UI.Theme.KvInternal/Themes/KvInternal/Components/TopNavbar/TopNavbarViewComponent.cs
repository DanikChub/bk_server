namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Themes.KvInternal.Components.TopNavbar;

using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

public class TopNavbarViewComponent : AbpViewComponent
{
    public IViewComponentResult Invoke() => this.View("~/Themes/KvInternal/Components/TopNavbar/Default.cshtml");
}
