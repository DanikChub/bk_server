namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class UserDto : EntityDto<Guid>
{
    public bool IsActive { get; set; }
    public string FullName { get; set; }
    public string JobPost { get; set; }
    public string Direction { get; set; } // направленая деятельность?
    public string TenantShortName { get; set; }
    public DateTime ActualizationTime { get; set; }
}
