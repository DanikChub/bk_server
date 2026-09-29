namespace KV.Server;

using System;
using Volo.Abp.Application.Dtos;

public class GetContractListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
    public string ManagerId { get; set; }
    public string SearchName { get; set; }
    public string SearchContractStartDate { get; set; }
    public string SearchContractFinishDate { get; set; }
    public string SearchStatusId { get; set; }
    public string SearchTenantProfileShortName { get; set; }
    public Guid? TenantId { get; set; }
    public string SearchServicePackageId { get; set; }
    public string SearchByActive { get; set; }
}
