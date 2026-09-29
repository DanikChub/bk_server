namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class GetEventListResultDto : PagedAndSortedResultRequestDto
{
    public DateTime StartPeriod { get; set; }
    public DateTime EndPeriod { get; set; }
    public Guid? TenantId { get; set; }
}
