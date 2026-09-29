namespace KV.Server.Dtos.Users;

using System;
using Volo.Abp.Application.Dtos;

public class CreateUpdateUserDto : EntityDto<Guid>
{

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }
    public string JobPost { get; set; }
    public string TenantShortName { get; set; }

    public DateTime DateOfBirth { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }

    public Guid? TenantId { get; set; }
}
