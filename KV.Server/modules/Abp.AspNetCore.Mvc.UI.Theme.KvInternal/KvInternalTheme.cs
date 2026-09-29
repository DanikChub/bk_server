namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal;

using Volo.Abp.AspNetCore.Mvc.UI.Theming;
using Volo.Abp.DependencyInjection;

[ThemeName(Name)]
public class KvInternalTheme : ITheme, ITransientDependency
{
    public const string Name = "KvInternal";

    public string GetLayout(string name, bool fallbackToDefault = true) => name switch
    {
        StandardLayouts.Application => "~/Themes/KvInternal/Layouts/Application.cshtml",
        StandardLayouts.Account => "~/Themes/KvInternal/Layouts/Account.cshtml",
        StandardLayouts.Empty => "~/Themes/KvInternal/Layouts/Empty.cshtml",
        _ => fallbackToDefault ? "~/Themes/KvInternal/Layouts/Application.cshtml" : null,
    };
}
