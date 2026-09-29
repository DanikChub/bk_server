using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KV.Server.File;
using KV.Server.Tickets;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace KV.Server.Profiles;
public class EmployeeProfile : AuditedAggregateRoot<Guid>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="identityUserId">Id пользователя</param>
    /// <param name="userAvatarFileId">Id аватара пользователя</param>
    /// <param name="jobPost">Должность</param>
    public EmployeeProfile(Guid identityUserId, Guid? userAvatarFileId, string jobPost, string phone)
    {
        IdentityUserId = identityUserId;
        UserAvatarFileId = userAvatarFileId;
        JobPost = jobPost;
        Phone = phone;
    }
    public DateTime? Birthday { get; protected set; }
    [MaxLength(256)]
    public virtual string FirstName { get; protected set; }

    /// <summary>
    ///     Фамилия
    /// </summary>
    [MaxLength(256)]
    public virtual string LastName { get; protected set; }

    [Phone]
    public virtual string Phone { get; protected set; }


    /// <summary>
    ///     Отчество
    /// </summary>
    [MaxLength(256)]
    public virtual string MiddleName { get; protected set; }

    /// <summary>
    ///     Email
    /// </summary>
    public virtual string Email { get; protected set; }

    /// <summary>
    ///     Ссылка на пользователя
    /// </summary>
    public virtual Guid IdentityUserId { get; protected set; }

    public virtual IdentityUser IdentityUser { get; protected set; }
    /// <summary>
    ///    Профиль фото
    /// </summary>
    public virtual Guid? UserAvatarFileId { get; protected set; }

    public virtual UploadedFile UserAvatarFile { get; protected set; }
    public virtual ICollection<Ticket> ResponsibleTickets { get; protected set; }
    /// <summary>
    ///     Изменить изображение профиля
    /// </summary>
    /// <param name="userAvatarFileId"></param>
    public EmployeeProfile UpdateProfileImage(Guid userAvatarFileId)
    {
        this.UserAvatarFileId = userAvatarFileId;
        return this;
    }

    public virtual string JobPost { get; protected set; }
    public EmployeeProfile SetJobPost(string jobPost)
    {
        this.JobPost = jobPost;

        return this;
    }
    public EmployeeProfile SetUserId(Guid identityUserId)
    {
        IdentityUserId = identityUserId;

        return this;
    }
    public EmployeeProfile SetAvatar(Guid avatarFileId)
    {
        UserAvatarFileId = avatarFileId;

        return this;
    }
    public EmployeeProfile SetFullName(string lastName, string firstName, string middleName)
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
    public EmployeeProfile SetDateOfBirthday(DateTime? dateOfBirthDay)
    {
        Birthday = dateOfBirthDay;
        return this;
    }

    public EmployeeProfile SetPhone(string phone)
    {
        this.Phone = phone;
        return this;
    }

    public EmployeeProfile SetEmail(string email)
    {
        this.Email = email;
        return this;
    }

    public string GetShortFullName()
    {
        return $"{LastName} {FirstName} {MiddleName}";
    }
}
