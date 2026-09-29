namespace KV.Server;
using Volo.Abp.Application.Dtos;

public class GetTicketSectionListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
}
