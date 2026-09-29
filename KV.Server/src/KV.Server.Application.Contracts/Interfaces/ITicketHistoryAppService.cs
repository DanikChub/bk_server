namespace KV.Server;
using System;
using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface ITicketHistoryAppService : IApplicationService
{
    Task<PagedResultDto<TicketHistoryDto>> GetTicketHistoryListAsync(GetTicketHistoryListRequestDto input,
        long ticketId);

    Task<TicketHistoryDto> CreateNotPickUpTicketHistoryAsync(long ticketId);
    Task<TicketHistoryDto> GetTicketHistoryAsync(Guid ticketHistoryId);
    Task UpdateTicketHistoryDescriptionAsync(UpdateTicketHistoryDescriptionDto updateDto);
    Task<TicketHistoryDto> CreateTicketHistoryAsync(CreateTicketHistoryDto dto);
    Task<TicketHistoryDto> CreateAdminTicketHistoryAsync(CreateAdminTicketHistoryDto dto);
    Task MakeReadTicketHistoryByIdAsync(Guid ticketHistoryId, UpdateReadTicketHistoryDto dto);
    Task<long> GetCountTicketHistoryAsync(long ticketId);
}
