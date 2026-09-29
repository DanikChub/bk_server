namespace KV.Server.PublicWeb.Pages.Tickets;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class ComposeModalModel : ServerPageModel
{
    public void OnGet()
    {
    }
}
