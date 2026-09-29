namespace KV.Server.PublicWeb;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

[Dependency(ReplaceServices = true)]
public class ServerBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "ИПС";
}
