namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class CreateUpdateCustomerUserProfileDto : AuditedEntityDto<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string JobPost { get; set; }
    public DateTime? DateOfBirthDay { get; set; }
    public Guid? UserAvatarFileId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid IdentityUserId { get; set; }

    public string Note { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }

    public string PhoneNumber { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
}
