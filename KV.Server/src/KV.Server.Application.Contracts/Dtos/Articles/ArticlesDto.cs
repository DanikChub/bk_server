namespace KV.Server;
using System;

public class ArticlesDto
{
    /// <summary>
    ///     Имя статьи.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Ссылка на полный текст статьи.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Тип статьи.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     Дата публикации.
    /// </summary>
    public DateTime PublishedAt { get; set; }
}
