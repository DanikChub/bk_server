namespace KV.Server;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface ITicketFavoriteAppService : IApplicationService
{
    Task<PagedResultDto<TicketClientFavoriteDto>> GetFavoritesClientAsync(GetTicketClientFavoriteListRequestDto input);

    Task<PagedResultDto<TicketSpecialistFavoriteDto>> GetFavoritesSpecialistAsync(
        GetTicketSpecialistFavoriteListRequestDto input);

    Task SetFavoriteClientAsync(long ticketId);
    Task<bool> GetIsFavoriteClientByTicketIdAsync(long ticketId);
    Task SetFavoriteSpecialistAsync(long ticketId);
    Task DeleteFavoriteClientAsync(long ticketId);
    Task DeleteFavoriteSpecialistAsync(long ticketId);
}
