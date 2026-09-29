using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Themes.KvInternal.Components.BreadCrumb;
public class ContentBreadCrumbViewComponent : AbpViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View("~/Themes/KvInternal/Components/BreadCrumb/Default.cshtml");
    }
}
