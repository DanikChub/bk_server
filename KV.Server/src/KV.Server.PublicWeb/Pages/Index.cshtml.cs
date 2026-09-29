namespace KV.Server.PublicWeb.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class IndexModel : ServerPageModel
{
    public IndexModel()
    {

    }

    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync() =>
        await this.HttpContext.ChallengeAsync("oidc");
}
