namespace KV.Server.Web.Pages;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class IndexModel : ServerPageModel
{
    private readonly IStoriesClientAppService _storiesClientApp;
    public IndexModel(IStoriesClientAppService storiesClientApp)
    {
        this._storiesClientApp = storiesClientApp;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnGetFileAsync(string id)
    {
        var stream = await this._storiesClientApp.GetDownloadAsync(id);
        return this.File(stream.GetStream(), stream.ContentType);
    }
}
