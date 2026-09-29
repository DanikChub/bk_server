namespace KV.Server;
using System;

public class CreateUpdateContractDto
{
    public string Name { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime ContractFinishDate { get; set; }
    public DateTime ContractStartDate { get; set; }
    public Guid? TenantId { get; set; }
    public Guid StatusId { get; set; }
    public string StatusCode { get; set; }
    public Guid? ServicePackageId { get; set; }
    public string ContractPayer { get; set; }
}
