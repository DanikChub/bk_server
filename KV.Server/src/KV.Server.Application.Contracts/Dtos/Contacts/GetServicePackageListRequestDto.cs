namespace KV.Server;
using Volo.Abp.Application.Dtos;

public class GetServicePackageListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
}
