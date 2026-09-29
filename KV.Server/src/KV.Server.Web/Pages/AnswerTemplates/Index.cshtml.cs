namespace KV.Server.Web.Pages.AnswerTemplates;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class IndexModel : ServerPageModel
{
    public void OnGet()
    {
    }
}
