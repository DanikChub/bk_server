namespace KV.Server.Templates;
using System;
using Volo.Abp.Domain.Entities;

/// <summary>
///     Шаблон ответа под заявкой
/// </summary>
public class AnswerTemplate : Entity<Guid>
{
    /// <summary>
    ///     Отображаемое имя
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Текст
    /// </summary>
    public string Description { get; set; }
}
