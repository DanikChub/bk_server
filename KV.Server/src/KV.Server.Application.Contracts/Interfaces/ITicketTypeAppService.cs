namespace KV.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using Volo.Abp.Application.Services;

public interface ITicketTypeAppService : IReadOnlyAppService<TicketTypeDto, Guid, GetTicketTypeListRequestDto>
{
    Task<List<TicketTypeDto>> GetTicketTypesAsync();
}
