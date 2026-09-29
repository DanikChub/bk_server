namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Themes.KvPublic.Components.ContentTitle;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Layout;

public class ContentTitleViewComponent : AbpViewComponent
{
    private readonly IPageLayout _pageLayout;

    public ContentTitleViewComponent(IPageLayout pageLayout)
    {
        this._pageLayout = pageLayout;
    }

    public virtual IViewComponentResult Invoke() => this.View("~/Themes/KvPublic/Components/ContentTitle/Default.cshtml", this._pageLayout.Content);
}
