namespace KV.Server.Profiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using KV.Server.File;
using KV.Server.Tickets;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

/// <summary>
///     Профиль пользователя
/// </summary>
public class CustomerUserProfile : AuditedAggregateRoot<Guid>, IMultiTenant
{
    protected CustomerUserProfile()
    {
    }

    public CustomerUserProfile(
        Guid id,
        Guid identityUserId,
        Guid? userAvatarFileId,
        Guid? tenantId
    ) : base(identityUserId)
    {
        this.Id = id;
        this.IdentityUserId = identityUserId;
        this.UserAvatarFileId = userAvatarFileId;
        this.TenantId = tenantId;
        this.ResponsibleTickets = new Collection<Ticket>();
    }

    /// <summary>
    ///     Ссылка на пользователя
    /// </summary>
    public virtual Guid IdentityUserId { get; protected set; }

    public virtual IdentityUser IdentityUser { get; protected set; }

    /// <summary>
    ///     Имя
    /// </summary>
    [MaxLength(256)]
    public virtual string FirstName { get; protected set; }

    /// <summary>
    ///     Фамилия
    /// </summary>
    [MaxLength(256)]
    public virtual string LastName { get; protected set; }

    /// <summary>
    ///     Отчество
    /// </summary>
    [MaxLength(256)]
    public virtual string MiddleName { get; protected set; }

    /// <summary>
    ///     Улица
    /// </summary>
    [MaxLength(512)]
    public virtual string Street { get; protected set; }

    /// <summary>
    ///     Город
    /// </summary>
    [MaxLength(256)]
    public virtual string City { get; protected set; }

    /// <summary>
    ///     Страна
    /// </summary>
    [MaxLength(256)]
    public virtual string Country { get; protected set; }

    /// <summary>
    ///     Рабочие назначение
    /// </summary>
    public virtual string JobPost { get; protected set; }

    /// <summary>
    ///     Дата рождение
    /// </summary>
    public virtual DateTime? DateOfBirthDay { get; protected set; }

    /// <summary>
    ///     Пользовательский файл, ссылка на профиль
    /// </summary>
    public virtual Guid? UserAvatarFileId { get; protected set; }

    public virtual string Email { get; protected set; }

    public virtual UploadedFile UserAvatarFile { get; protected set; }

    /// <summary>
    ///     Заметки пользователя
    /// </summary>
    public virtual string Note { get; protected set; }

    public virtual TenantProfile TenantProfile { get; protected set; }

    /// <summary>
    ///     Коллекция заявок пользователя
    /// </summary>
    public virtual ICollection<Ticket> ResponsibleTickets { get; protected set; }

    public virtual ICollection<TenantProfileManager> TenantProfileManagers { get; protected set; }
    public virtual ICollection<TicketMessage> TicketCreatorMessages { get; protected set; }
    public virtual ICollection<TicketClientFavorite> TicketClientFavorites { get; protected set; }
    public virtual ICollection<TicketSpecialistFavorite> TicketSpecialistFavorites { get; protected set; }

    /// <summary>
    ///     Ссылка на тенант
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    ///     Установить адрес
    /// </summary>
    /// <param name="country"></param>
    /// <param name="city"></param>
    /// <param name="street"></param>
    public CustomerUserProfile SetAddress(string country, string city, string street)
    {
        this.Street = street;
        this.City = city;
        this.Country = country;

        return this;
    }

    /// <summary>
    ///     Написать заметку
    /// </summary>
    /// <param name="note"></param>
    public void WriteNote(string note) => this.Note = note;

    /// <summary>
    ///     Установить полное имя
    /// </summary>
    /// <param name="lastName"></param>
    /// <param name="firstName"></param>
    /// <param name="middleName"></param>
    public CustomerUserProfile SetFullName(string lastName, string firstName, string middleName)
    {
        this.FirstName = firstName;
        this.MiddleName = middleName;
        this.LastName = lastName;

        if (this.IdentityUser != null)
        {
            this.IdentityUser.Name = firstName;
            this.IdentityUser.Surname = lastName;
        }

        return this;
    }

    /// <summary>
    ///     Установить день рождение
    /// </summary>
    /// <param name="dateOfBirthDay"></param>
    public CustomerUserProfile SetDateOfBirthday(DateTime? dateOfBirthDay)
    {
        this.DateOfBirthDay = dateOfBirthDay;
        return this;
    }

    /// <summary>
    ///     Установить рабочие место
    /// </summary>
    /// <param name="jobPost"></param>
    public CustomerUserProfile SetJobPost(string jobPost)
    { 
        this.JobPost = jobPost;
        return this;
    }

    /// <summary>
    ///     Изменить изображение профиля
    /// </summary>
    /// <param name="userAvatarFileId"></param>
    public CustomerUserProfile SetAvatar(Guid userAvatarFileId)
    {
        this.UserAvatarFileId = userAvatarFileId;
        return this;
    }

    public CustomerUserProfile SetEmail(string email)
    {
        this.Email = email;
        return this;
    }

    public string GetShortFullName()
    {
        if (string.IsNullOrWhiteSpace(this.LastName))
        {
            return $"{this.FirstName} {this.MiddleName}";
        }

        var result = this.LastName;
        if (!string.IsNullOrWhiteSpace(this.FirstName))
        {
            result += $" {this.FirstName.FirstOrDefault()}.";
        }

        if (!string.IsNullOrWhiteSpace(this.MiddleName))
        {
            result += $" {this.MiddleName.FirstOrDefault()}.";
        }

        return result;
    }
}
