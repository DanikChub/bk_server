namespace KV.Server;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

public class CustomerServicePackageDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Style { get; set; }
    public string ContractName { get; set; }
    public DateTime FinishDate { get; set; }
}
