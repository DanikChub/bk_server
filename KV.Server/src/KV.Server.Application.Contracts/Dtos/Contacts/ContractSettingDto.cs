namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class ContractSettingDto : AuditedEntityDto<Guid>
{
    public int Max { get; set; }
    public string ConstraintTypeName { get; set; }
    public Guid ConstraintTypeId { get; set; }
    public Guid ContractId { get; set; }
}
