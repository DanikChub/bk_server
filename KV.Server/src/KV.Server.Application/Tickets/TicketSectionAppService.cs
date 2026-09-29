namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

public class TicketSectionAppService : ApplicationService, ITicketSectionAppService
{
    private readonly IRepository<TicketSection, Guid> _ticketSectionRepository;

    public TicketSectionAppService(IRepository<TicketSection, Guid> ticketSectionRepository)
    {
        this._ticketSectionRepository = ticketSectionRepository;
    }

    public async Task<List<TicketSectionDto>> GetTicketSectionsAsync()
    {
        var query = await this._ticketSectionRepository.GetQueryableAsync();
        var entites = query.ToList();
        var dtos = this.ObjectMapper.Map<List<TicketSection>, List<TicketSectionDto>>(entites);
        return dtos;
    }
}
