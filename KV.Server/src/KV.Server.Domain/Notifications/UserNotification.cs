namespace KV.Server.Notifications;
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

public class UserNotification : AuditedEntity<Guid>
{
    public UserNotification(string title, string url, Guid identityUserId, Guid? creatorId,
        Guid? notificationCategoryId = null)
    {
        this.Title = title;
        this.Url = url;
        this.IdentityUserId = identityUserId;
        this.CreatorId = creatorId;
        this.NotificationCategoryId = notificationCategoryId;
    }

    /// <summary>
    ///     Заголовок
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Время прочтения (пользователям)
    /// </summary>
    public DateTime? Seen { get; set; }

    /// <summary>
    ///     Ссылка для перехода с уведомления на нужную страницу
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Пользователь получивший сообщение
    /// </summary>
    public Guid IdentityUserId { get; set; }

    public IdentityUser IdentityUser { get; set; }

    /// <summary>
    ///     Категория уведомления
    /// </summary>
    public Guid? NotificationCategoryId { get; set; }

    public NotificationCategory NotificationCategory { get; set; }

    /// <summary>
    ///     Создатель (может и не быть)
    /// </summary>
    public IdentityUser Creator { get; set; }
}
