namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Themes.KvInternal.Components.LeftNavbar;

using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

public class LeftNavbarViewComponent : AbpViewComponent
{
    public IViewComponentResult Invoke() => this.View("~/Themes/KvInternal/Components/LeftNavbar/Default.cshtml");
}
