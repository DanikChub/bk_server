namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using Volo.Abp.Application.Services;

public interface ITicketStatusAppService : IReadOnlyAppService<TicketStatusDto, Guid, GetTicketStatusListRequestDto>
{
    Task<List<TicketStatusInfoDto>> GetTicketStatusByCreatorIdAndTenantAsync(Guid? creatorId);
    Task<List<TicketStatusDto>> GetListStatusesAsync();
    Task<TicketStatusInfoDto> GetMyTicketStatusByResponsibleIdAsync(Guid? responsibleId);
    Task<TicketStatusInfoDto> GetTicketFavoriteInfoAsync();
    Task<long> GetCountTicketByTicketStatusAsync(Guid? ticketStatusId);
}
