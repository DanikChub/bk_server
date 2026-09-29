using System;
using System.Collections.Generic;
using KV.Server.Dtos.File;
using Volo.Abp.Application.Dtos;

namespace KV.Server.Dtos.Employees;
public class CreateUpdateEmployeeProfileDto : AuditedEntityDto<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string JobPost { get; set; }
    public string Phone { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime Birthday { get; set; }
    public Guid? UserAvatarFileId { get; set; }
    public Guid IdentityUserId { get; set; }
    public List<string> RoleNames { get; set; }
    public UploadedFileDto AvatarFile { get; set; }
}
