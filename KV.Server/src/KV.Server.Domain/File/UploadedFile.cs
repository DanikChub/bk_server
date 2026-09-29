namespace KV.Server.File;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

/// <summary>
///     Файл для загрузки
/// </summary>
public class UploadedFile : AuditedEntity<Guid>
{
    /// <summary>
    ///     Заголовок файла
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Название файла
    /// </summary>
    [MaxLength(1024)]
    public string FileName { get; set; }

    /// <summary>
    ///     Медиа тип
    /// </summary>
    [MaxLength(1024)]
    public string ContentType { get; set; }

    /// <summary>
    ///     Размер в байтах
    /// </summary>
    public long SizeInBytes { get; set; }

    /// <summary>
    ///     Оригинальное название
    /// </summary>
    [StringLength(1024)]
    public string OriginalFileName { get; set; }

    /// <summary>
    ///     Провайдер доступа к хранилищу файлов
    /// </summary>
    [StringLength(100)]
    public string StorageProvider { get; set; }
}
