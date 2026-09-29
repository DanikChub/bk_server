namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class GetCustomerEventsListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
    public Guid TenantId { get; set; }
    public DateTime StartPeriod { get; set; }
    public DateTime EndPeriod { get; set; }
}
