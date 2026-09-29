namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class ServicePackageDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Style { get; set; }
}
