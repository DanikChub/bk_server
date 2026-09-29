using System;

namespace KV.Server.Dtos.Contacts;
public class ServicePackageDataDto
{
    public string? Name { get; set; }
    public string? Style { get; set; }
    public string? Color { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractFinishDate { get; set; }

}
