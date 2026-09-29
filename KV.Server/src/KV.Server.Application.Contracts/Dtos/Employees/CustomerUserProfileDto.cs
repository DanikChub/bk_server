namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class CustomerUserProfileDto : AuditedEntityDto<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string JobPost { get; set; }
    public string TenantProfileDisplayName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string UserName { get; set; }
    public string INN { get; set; }
    public string KPP { get; set; }
    public string Site { get; set; }
    public string Note { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public bool IsActive { get; set; }

    public DateTime? DateOfBirthDay { get; set; }
    public Guid? UserAvatarFileId { get; set; }
    public Guid IdentityUserId { get; set; }
    public Guid? TenantId { get; set; }
}
