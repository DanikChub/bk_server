namespace KV.Server.Notifications;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

public class NotificationCategory : Entity<Guid>
{
    /// <summary>
    ///     Название
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Стиль отображения в виде css
    /// </summary>
    public string Style { get; set; }

    public ICollection<UserNotification> UserNotifications { get; set; }
}
