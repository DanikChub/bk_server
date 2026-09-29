namespace KV.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KV.Server.Profiles;
using KV.Server.Tenants;
using KV.Server.Tickets;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

/* Сущность для хранения дополнительной иноформации о тенантах.
 * В БД связывается со стандартной сущностью хранящей в себе 
 * имя и уникальный номер тенанта по внешнему ключу.*/
/// <summary>
///     Отдельная сущность для хранения информации о тенантах.
/// </summary>
public class TenantProfile : FullAuditedAggregateRoot<Guid>, ISoftDelete
{
    public TenantProfile()
    {
    }

    public TenantProfile(Guid id, string nameShort, string nameLong, string address)
    {
        this.Id = id;
        this.ShortName = nameShort;
        this.LongName = nameLong;
        this.Address = address;
    }

    /// <summary>
    ///     Краткое наименование тенанта.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string ShortName { get; set; }

    /// <summary>
    ///     Полное наименование тенанта.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string LongName { get; set; }

    /// <summary>
    ///     Адрес тенанта.
    /// </summary>
    [MaxLength(2000)]
    public string Address { get; set; }

    /// <summary>
    ///     Ссылка на сайт тенант
    /// </summary>
    public string SiteUrl { get; set; }

    /// <summary>
    ///     Описание тенанта
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     ИНН
    /// </summary>
    public string INNNumber { get; set; }

    /// <summary>
    ///     KPP
    /// </summary>
    public string KPPNumber { get; set; }

    /// <summary>
    ///     Заметка кто ответственен
    /// </summary>
    public string CommentNotes { get; set; }

    /// <summary>
    ///     Офицальное имя
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     Активен
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Владелец компании
    /// </summary>
    public CustomerUserProfile ContactUserProfile { get; set; }

    /// <summary>
    ///     Регион компании
    /// </summary>
    public Guid? RegionId { get; set; }

    public Region Region { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Ответственный по менеджерам (назначается при создание клиента)
    /// </summary>
    public Guid? ResponsibleManagerId { get; set; }

    public EmployeeProfile ResponsibleManager { get; set; }

    public ICollection<Contract> Contracts { get; set; }
    public ICollection<Ticket> Tickets { get; set; }
    public ICollection<TenantProfileManager> TenantProfileManagers { get; set; }
    public ICollection<CustomerUserProfile> CustomerUserProfiles { get; set; }

    public virtual string ServicePackagesData { get; protected set; }
}
