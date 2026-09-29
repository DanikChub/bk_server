namespace KV.Server;
using System;

public class CreateUpdateContractStatusDto
{
    public Guid? TenantId { get; set; }
    public string Title { get; set; } = "Draft";
    public string Code { get; set; } = "0001";
}
