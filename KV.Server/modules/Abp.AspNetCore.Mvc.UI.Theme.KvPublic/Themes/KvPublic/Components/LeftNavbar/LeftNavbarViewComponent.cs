namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Themes.KvPublic.Components.LeftNavbar;

using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

public class LeftNavbarViewComponent : AbpViewComponent
{
    public IViewComponentResult Invoke() => this.View("~/Themes/KvPublic/Components/LeftNavbar/Default.cshtml");
}
