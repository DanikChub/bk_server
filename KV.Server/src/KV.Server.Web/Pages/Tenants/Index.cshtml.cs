namespace Server.Web.Pages.Tenants;
using KV.Server.Web.Pages;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class IndexModel : ServerPageModel
{
    public void OnGet()
    {
    }
}
