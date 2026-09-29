namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;

public class ContractStatusDto : FullAuditedEntityDto<Guid>, IMultiTenant
{
    public string Title { get; set; }
    public string Code { get; set; }
    public Guid? TenantId { get; set; }
}
