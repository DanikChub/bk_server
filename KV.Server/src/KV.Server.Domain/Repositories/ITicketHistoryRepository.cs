using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KV.Server.Tickets;
using Volo.Abp.Domain.Repositories;

namespace KV.Server.Repositories;
public interface ITicketHistoryRepository : IBasicRepository<TicketHistory, Guid>
{
    Task<IQueryable<TicketHistory>> IncludeTicketWithCreatorAsync();
}
