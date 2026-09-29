namespace KV.Server;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;

public class ContractDto : FullAuditedEntityDto<Guid>, IMultiTenant
{
    public string Name { get; set; }
    public string TenantProfileShortName { get; set; }
    public DateTime ContractStartDate { get; set; }
    public DateTime ContractFinishDate { get; set; }
    public Guid? StatusId { get; set; }
    public string StatusTitle { get; set; }
    public string StatusCode { get; set; }
    public string ServicePackageName { get; set; } = "";
    public string ServicePackagesData { get; set; }
    public string ContractPayer { get; set; }
    public List<ServicePackageDto> ServicePackages { get; set; }
    public List<ConstraintTypeContractDto> ConstraintTypeContracts { get; set; }
    public Guid? TenantId { get; set; }
    public List<CustomerContractStatisticsDto> ContractStatistics { get; set; }
}
