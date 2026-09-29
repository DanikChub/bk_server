namespace KV.Server;
using System;
using System.Collections.Generic;

public class NewsDto
{
    /// <summary>
    ///     Имя новости.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Ссылка на полный текст новости.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Часть полного текста, для отображения.
    /// </summary>
    public string ShortContent { get; set; }

    /// <summary>
    ///     Ссылки на обложки новости различных размеров.
    /// </summary>
    public string CoverUrls { get; set; }

    /// <summary>
    ///     Перечень нарправлений для специалистов.
    /// </summary>
    public List<string> Purpose { get; set; }

    /// <summary>
    ///     Дата публикации.
    /// </summary>
    public DateTime PublishedAt { get; set; }
}
