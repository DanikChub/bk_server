namespace KV.Server;
using KV.Server.Localization;
using Volo.Abp.Application.Services;

/* Inherit your application services from this class.
 */
public abstract class ServerAppService : ApplicationService
{
    protected ServerAppService()
    {
        this.LocalizationResource = typeof(ServerResource);
    }
}
