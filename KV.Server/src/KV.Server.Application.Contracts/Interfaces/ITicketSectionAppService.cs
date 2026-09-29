namespace KV.Server;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface ITicketSectionAppService : IApplicationService
{
    Task<List<TicketSectionDto>> GetTicketSectionsAsync();
}
