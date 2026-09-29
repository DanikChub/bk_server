namespace KV.Server;
using System.Threading.Tasks;
using KV.Server.Dtos.Employees;
using KV.Server.Dtos.Tickets;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface ITicketsPublicAppService : IApplicationService
{
    Task<TicketRatingDto> GetTicketRatingAsync(long ticketId);
    Task<PagedResultDto<TicketListItemDto>> GetTicketsListAsync(GetTicketsListRequestDto input);
    Task<TicketDetailsDto> GetTicketDetailsAsync(long ticketId);
    Task<TicketDetailsDto> CreateTicketAsync(CreateUpdateTicketDto dto);
    Task UpdateTicketDraftAsync(long id, CreateUpdateTicketDto dto);
    Task<EmployeeProfileDto> GetSpecialistByTicketIdAsync(long ticketId);
    Task<TicketHistoryDto> CreateTicketHistoryAsync(CreateTicketHistoryDto dto);
    Task MakeReadTicketByIdAsync(long ticketId, UpdateReadTicketDto dto);
    Task SetRatingTicketAsync(long ticketId, CreateUpdateTicketRatingDto dto);

}
