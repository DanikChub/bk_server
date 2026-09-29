namespace KV.Server;
using System;
using Volo.Abp.Domain.Entities.Auditing;

/// <summary>
///     Сущность для хранения информации об историях.
/// </summary>
public class StoryItem : AuditedEntity<Guid>
{
    public StoryItem(Guid id, string coverImageUrl, int orderIndex, string groupName, string name, StoryStatus status)
    {
        this.Id = id;
        this.CoverImageUrl = coverImageUrl;
        this.OrderIndex = orderIndex;
        this.GroupName = groupName;
        this.Name = name;
        this.Status = status;
    }

    /// <summary>
    ///     Изображение обложки.
    /// </summary>
    public string CoverImageUrl { get; set; }

    /// <summary>
    ///     Порядковый номер сторис.
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    ///     Название сторис. (Новости, Услуги)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Расположение. (Left, Right)
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    ///     Признак публикации. (Active, Archive)
    /// </summary>
    public StoryStatus Status { get; set; }
}
