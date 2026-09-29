namespace KV.Server.Controllers;

using KV.Server.Localization;
using Volo.Abp.AspNetCore.Mvc;

/* Inherit your controllers from this class.
 */
public abstract class ServerController : AbpController
{
    protected ServerController()
    {
        this.LocalizationResource = typeof(ServerResource);
    }
}
