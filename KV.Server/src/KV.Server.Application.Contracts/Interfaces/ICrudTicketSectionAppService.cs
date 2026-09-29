namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface ICrudTicketSectionAppService : ICrudAppService<TicketSectionDto, Guid, GetTicketSectionListRequestDto,
    CreateUpdateTicketSectionDto>
{
    Task<List<TicketSectionDto>> GetListTicketSectionsAsync();
}
