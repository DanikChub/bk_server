namespace KV.Server;
using Volo.Abp.Application.Dtos;

public class GetTicketMessagesListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
    public long TicketId { get; set; }
}
