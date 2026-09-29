namespace KV.Server.Dtos.Tickets;
using Volo.Abp.Application.Dtos;

public class GetTicketHistoryListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
}
