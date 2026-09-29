namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

public class TicketTypeAppService : ReadOnlyAppService<TicketType, TicketTypeDto, Guid, GetTicketTypeListRequestDto>,
    ITicketTypeAppService
{
    public TicketTypeAppService(IReadOnlyRepository<TicketType, Guid> repository) : base(repository)
    {
    }

    public async Task<List<TicketTypeDto>> GetTicketTypesAsync()
    {
        var ticketType = await this.Repository.ToListAsync();
        var dtos = this.ObjectMapper.Map<List<TicketType>, List<TicketTypeDto>>(ticketType);
        return dtos;
    }
}
