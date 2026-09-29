namespace KV.Server;
using Volo.Abp.Application.Dtos;

public class GetTicketClientFavoriteListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
}
