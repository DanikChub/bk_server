using System;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.EntityFrameworkCore;
using KV.Server.Tickets;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace KV.Server.Repositories;
public class TicketHistoryRepository : EfCoreRepository<ServerDbContext, TicketHistory, Guid>, ITicketHistoryRepository
{
    public TicketHistoryRepository(IDbContextProvider<ServerDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<IQueryable<TicketHistory>> IncludeTicketWithCreatorAsync()
    {
        var dbSet = await this.GetDbSetAsync<TicketHistory>();
        var query = dbSet
            .Include(x => x.Creator)
            .Include(x => x.Ticket);
        return query;
    }
}
