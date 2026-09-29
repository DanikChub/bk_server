namespace KV.Server.Web.Pages.TicketSections;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class IndexModel : ServerPageModel
{
    public void OnGet()
    {
    }
}
