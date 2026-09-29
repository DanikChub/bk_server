namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using KV.Server.Permissions.Tickets;
using KV.Server.Profiles;
using KV.Server.Tickets;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

public class TicketFavoriteAppService : ApplicationService, ITicketFavoriteAppService
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<CustomerUserProfile, Guid> _customerUserProfileRepository;
    private readonly IRepository<TicketClientFavorite> _ticketClientFavoriteRepository;
    private readonly IRepository<TicketSpecialistFavorite> _ticketSpecialistFavoriteRepository;

    public TicketFavoriteAppService(IRepository<CustomerUserProfile, Guid> customerUserProfileRepository,
        IRepository<TicketClientFavorite> ticketClientFavoriteRepository,
        IRepository<TicketSpecialistFavorite> ticketSpecialistFavoriteRepository,
        ICurrentUser currentUser)
    {
        this._customerUserProfileRepository = customerUserProfileRepository;
        this._ticketClientFavoriteRepository = ticketClientFavoriteRepository;
        this._ticketSpecialistFavoriteRepository = ticketSpecialistFavoriteRepository;
        this._currentUser = currentUser;
    }

    public async Task SetFavoriteClientAsync(long ticketId)
    {
        var currentUserId = this._currentUser?.Id ?? Guid.Empty;
        if (currentUserId != Guid.Empty)
        {
            var customerUserProfile =
                await this._customerUserProfileRepository.FirstOrDefaultAsync(x => x.IdentityUserId == currentUserId);
            var customerUserProfileId = customerUserProfile?.Id ?? Guid.Empty;
            var existsFavorite = await this._ticketClientFavoriteRepository
                .FirstOrDefaultAsync(x => x.ClientId == customerUserProfileId && x.TicketId == ticketId);
            if (existsFavorite == null)
            {
                await this._ticketClientFavoriteRepository.InsertAsync(new TicketClientFavorite
                {
                    ClientId = customerUserProfileId,
                    TicketId = ticketId
                });
            }
        }
    }

    public async Task DeleteFavoriteClientAsync(long ticketId)
    {
        var currentUserId = this._currentUser?.Id ?? Guid.Empty;
        var customerUserProfile =
            await this._customerUserProfileRepository.FirstOrDefaultAsync(x => x.IdentityUserId == currentUserId);
        var customerUserProfileId = customerUserProfile?.Id ?? Guid.Empty;
        var client = await this._ticketClientFavoriteRepository
            .FirstOrDefaultAsync(x => x.ClientId == customerUserProfileId && x.TicketId == ticketId);
        if (client != null)
        {
            await this._ticketClientFavoriteRepository.DeleteAsync(client);
        }
    }

    public async Task<PagedResultDto<TicketClientFavoriteDto>> GetFavoritesClientAsync(
        GetTicketClientFavoriteListRequestDto input)
    {
        var currentUserId = this._currentUser?.Id ?? Guid.Empty;
        var customerUserProfile =
            await this._customerUserProfileRepository.FirstOrDefaultAsync(x => x.IdentityUserId == currentUserId);
        var customerUserProfileId = customerUserProfile?.Id ?? Guid.Empty;
        var query = await this._ticketClientFavoriteRepository.GetQueryableAsync();
        query = query.Where(x => x.ClientId == customerUserProfileId);
        var count = query.Count();
        var entities = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount == 0 ? GetTicketsListRequestDto.DefaultPageSize : input.MaxResultCount)
            .ToList();
        var ticketsDtos = this.ObjectMapper.Map<List<TicketClientFavorite>, List<TicketClientFavoriteDto>>(entities);
        return new PagedResultDto<TicketClientFavoriteDto>(count, ticketsDtos);
    }

    public async Task SetFavoriteSpecialistAsync(long ticketId)
    {
        var currentUserId = this._currentUser?.Id ?? Guid.Empty;
        var existsFavorite = await this._ticketSpecialistFavoriteRepository
            .FirstOrDefaultAsync(x => x.SpecialistId == currentUserId && x.TicketId == ticketId);
        if (existsFavorite == null)
        {
            await this._ticketSpecialistFavoriteRepository.InsertAsync(new TicketSpecialistFavorite
            {
                SpecialistId = currentUserId,
                TicketId = ticketId
            });
        }
    }

    [Authorize(TicketPermissions.Tickets.Delete)]
    public async Task DeleteFavoriteSpecialistAsync(long ticketId)
    {
        var currentUserId = this._currentUser?.Id ?? Guid.Empty;
        var specialist = await this._ticketSpecialistFavoriteRepository
            .FirstOrDefaultAsync(x => x.SpecialistId == currentUserId && x.TicketId == ticketId);
        if (specialist != null)
        {
            await this._ticketSpecialistFavoriteRepository.DeleteAsync(specialist);
        }
    }

    [Authorize(TicketPermissions.Tickets.Get)]
    public async Task<PagedResultDto<TicketSpecialistFavoriteDto>> GetFavoritesSpecialistAsync(
        GetTicketSpecialistFavoriteListRequestDto input)
    {
        var currentUserId = this._currentUser?.Id ?? Guid.Empty;
        var query = await this._ticketSpecialistFavoriteRepository.GetQueryableAsync();
        query = query.Where(x => x.SpecialistId == currentUserId);
        var count = query.Count();
        var entities = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount == 0 ? GetTicketsListRequestDto.DefaultPageSize : input.MaxResultCount)
            .ToList();
        var ticketsDtos = this.ObjectMapper.Map<List<TicketSpecialistFavorite>, List<TicketSpecialistFavoriteDto>>(entities);
        return new PagedResultDto<TicketSpecialistFavoriteDto>(count, ticketsDtos);
    }
    [Authorize(TicketPermissions.Tickets.Get)]
    public async Task<bool> GetIsFavoriteClientByTicketIdAsync(long ticketId)
    {
        var currentUserId = this._currentUser?.Id ?? Guid.Empty;
        var query = (await this._ticketSpecialistFavoriteRepository.WithDetailsAsync()).Where(x => x.TicketId == ticketId).FirstOrDefault();

        return query != null;
    }
}
