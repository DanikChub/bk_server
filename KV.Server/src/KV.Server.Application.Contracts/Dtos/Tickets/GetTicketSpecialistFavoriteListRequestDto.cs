namespace KV.Server;
using Volo.Abp.Application.Dtos;

public class GetTicketSpecialistFavoriteListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
}
