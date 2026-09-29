namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Themes.KvPublic.Components.TopNavbar;

using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

public class TopNavbarViewComponent : AbpViewComponent
{
    public IViewComponentResult Invoke() => this.View("~/Themes/KvPublic/Components/TopNavbar/Default.cshtml");
}
