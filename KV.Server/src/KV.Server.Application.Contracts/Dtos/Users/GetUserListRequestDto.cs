namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class GetUserListRequestDto : PagedAndSortedResultRequestDto
{
    public string SearchStr { get; set; }
    public string SearchFullName { get; set; }
    public string SearchJobPost { get; set; }
    public string SearchDirection { get; set; }
    public string SearchTenantShortName { get; set; }
    public string SearchResponsibleManagerId { get; set; }
    public DateTime? SearchActualizationTime { get; set; }
    public string SearchByActive { get; set; }
}
