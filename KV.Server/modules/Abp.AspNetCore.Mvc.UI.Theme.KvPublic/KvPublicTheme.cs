namespace Abp.AspNetCore.Mvc.UI.Theme.KvPublic;

using Volo.Abp.AspNetCore.Mvc.UI.Theming;
using Volo.Abp.DependencyInjection;

[ThemeName(Name)]
public class KvPublicTheme : ITheme, ITransientDependency
{
    public const string Name = "KvPublic";

    public string GetLayout(string name, bool fallbackToDefault = true) => name switch
    {
        StandardLayouts.Application => "~/Themes/KvPublic/Layouts/Application.cshtml",
        StandardLayouts.Account => "~/Themes/KvPublic/Layouts/Account.cshtml",
        StandardLayouts.Empty => "~/Themes/KvPublic/Layouts/Empty.cshtml",
        _ => fallbackToDefault ? "~/Themes/KvPublic/Layouts/Application.cshtml" : null,
    };
}
