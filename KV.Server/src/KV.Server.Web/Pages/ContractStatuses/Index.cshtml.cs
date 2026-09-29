namespace KV.Server.Web.Pages.ContractStatuses;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class IndexModel : ServerPageModel
{
    public void OnGet()
    {
    }
}
