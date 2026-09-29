namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class GetCustomerManagerListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
    public Guid CustomerId { get; set; }
}
