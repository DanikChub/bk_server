namespace KV.Server.Notifications;
using System;
using Volo.Abp.Domain.Entities;

public class NotificationStatus : Entity<Guid>
{
    /// <summary>
    ///     Название
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Отображение имени
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     Стиль отображения в виде css
    /// </summary>
    public string Style { get; set; }

    /// <summary>
    ///     Отображение в виде множественного числа
    /// </summary>
    public string DisplayNameMany { get; set; }

    /// <summary>
    ///     Иконка
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    ///     Перечисления для типа уведомления
    /// </summary>
    public NotificationStatuses Status { get; set; }
}
