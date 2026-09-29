namespace KV.Server;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface ITicketMessageAppService : IApplicationService
{
    Task SendMessageAsync(CreateUpdateTicketMessageDto message);
    Task<PagedResultDto<TicketMessageDto>> GetListAsync(GetTicketMessagesListRequestDto input);
    Task<bool> ExistAnyTicketMessageAsync(long ticketId);
    Task<int> GetCountTicketMessageAsync(long ticketId);
}
