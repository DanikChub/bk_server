namespace KV.Server;
using System;

public class CustomerEventDto
{
    /// <summary>
    ///     Совершивший событие Имя
    /// </summary>
    public string CreatorFirstName { get; set; }

    /// <summary>
    ///     Совершивший событие Фамилия
    /// </summary>
    public string CreatorLastName { get; set; }

    /// <summary>
    ///     3 действия - Оставил коментарий к заявке, назначил ответственного, закрыл заявку
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    ///     Тема заявки Пример: “О проведении противопаводковых мероприятий”
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    ///     Дата создание события
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    ///     редирект, при нажатие на кнопку
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///    Описание
    /// </summary>
    public string? Description { get; set; }
}
