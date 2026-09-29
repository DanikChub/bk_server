namespace KV.Server;
using System;
using Volo.Abp.Domain.Entities.Auditing;

/// <summary>
///     Сущность для хранения информации о слайдах историй.
/// </summary>
public class SlideItem : AuditedEntity<Guid>
{
    public SlideItem(Guid id, Guid storyId, string imageUrl, int orderIndex)
    {
        this.Id = id;
        this.StoryId = storyId;
        this.ImageUrl = imageUrl;
        this.OrderIndex = orderIndex;
    }

    /// <summary>
    ///     Внешний ключ истории.
    /// </summary>
    public Guid StoryId { get; set; }

    /// <summary>
    ///     Изображение слайда.
    /// </summary>
    public string ImageUrl { get; set; }

    /// <summary>
    ///     Порядковый номер слайда.
    /// </summary>
    public int OrderIndex { get; set; }
}
