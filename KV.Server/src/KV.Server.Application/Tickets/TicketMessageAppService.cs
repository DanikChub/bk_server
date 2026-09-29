namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Permissions.TicketMessages;
using KV.Server.Tickets;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

[Authorize]
public class TicketMessageAppService : ApplicationService, ITicketMessageAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<TicketMessage, Guid> _ticketMessageRepository;

    public TicketMessageAppService(IRepository<TicketMessage, Guid> ticketMessageRepository,
        IDataFilter dataFilter)
    {
        this._ticketMessageRepository = ticketMessageRepository;
        this._dataFilter = dataFilter;
    }

    [Authorize(TicketMessagePermissions.TicketMessages.Create)]
    public async Task SendMessageAsync(CreateUpdateTicketMessageDto message)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticketMessage = this.ObjectMapper.Map<CreateUpdateTicketMessageDto, TicketMessage>(message);
            await this._ticketMessageRepository.InsertAsync(ticketMessage);
        }
    }

    [Authorize(TicketMessagePermissions.TicketMessages.Get)]
    public async Task<PagedResultDto<TicketMessageDto>> GetListAsync(GetTicketMessagesListRequestDto input)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._ticketMessageRepository.WithDetailsAsync(
                x => x.CreatorCustomerUserProfile,
                x => x.CreatorCustomerUserProfile.IdentityUser);
            query = query.Where(x => x.TicketId == input.TicketId);
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }

            var totalCount = query.Count();
            var entities = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount == 0
                    ? GetTicketMessagesListRequestDto.DefaultPageSize
                    : input.MaxResultCount)
                .ToList();
            var dtos = this.ObjectMapper.Map<List<TicketMessage>, List<TicketMessageDto>>(entities);
            return new PagedResultDto<TicketMessageDto>(totalCount, dtos);
        }
    }

    [Authorize(TicketMessagePermissions.TicketMessages.Get)]
    public async Task<int> GetCountTicketMessageAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var count = await this._ticketMessageRepository.CountAsync(x => x.TicketId == ticketId);
            return count;
        }
    }

    [Authorize]
    public async Task<bool> ExistAnyTicketMessageAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var exist = await this._ticketMessageRepository.AnyAsync(x => x.TicketId == ticketId);
            return exist;
        }
    }
}
