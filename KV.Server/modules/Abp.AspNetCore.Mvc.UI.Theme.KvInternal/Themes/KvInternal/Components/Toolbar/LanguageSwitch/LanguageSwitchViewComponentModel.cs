namespace Abp.AspNetCore.Mvc.UI.Theme.KvInternal.Themes.KvInternal.Components.Toolbar.LanguageSwitch;

using System.Collections.Generic;
using Volo.Abp.Localization;

public class LanguageSwitchViewComponentModel
{
    public LanguageInfo CurrentLanguage { get; set; }

    public List<LanguageInfo> OtherLanguages { get; set; }
}
