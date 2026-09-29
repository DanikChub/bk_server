namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Themes.KvPublic.Components.PageAlerts;

using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Alerts;

public class PageAlertsViewComponent : AbpViewComponent
{
    private readonly IAlertManager _alertManager;

    public PageAlertsViewComponent(IAlertManager alertManager)
    {
        this._alertManager = alertManager;
    }

    public IViewComponentResult Invoke(string name) => this.View("~/Themes/KvPublic/Components/PageAlerts/Default.cshtml", this._alertManager.Alerts);
}
