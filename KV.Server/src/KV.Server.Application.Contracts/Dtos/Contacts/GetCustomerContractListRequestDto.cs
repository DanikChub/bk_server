namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class GetCustomerContractListRequestDto : PagedAndSortedResultRequestDto
{
    public Guid TenantId { get; set; }
    public string SearchName { get; set; }
    public string SearchContractStartDate { get; set; }
    public string SearchContractFinishDate { get; set; }
    public string SearchStatusId { get; set; }
}
