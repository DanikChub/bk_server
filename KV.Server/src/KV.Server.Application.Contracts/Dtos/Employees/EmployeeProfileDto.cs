using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace KV.Server.Dtos.Employees;
public class EmployeeProfileDto : AuditedEntityDto<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string JobPost { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string NewPassword { get; set; }
    public DateTime Birthday { get; set; }
    public Guid? UserAvatarFileId { get; set; }
    public Guid IdentityUserId { get; set; }
}
