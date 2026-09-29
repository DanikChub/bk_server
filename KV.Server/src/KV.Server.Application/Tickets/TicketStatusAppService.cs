namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using KV.Server.Profiles;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

public class TicketStatusAppService :
    ReadOnlyAppService<TicketStatus, TicketStatusDto, Guid, GetTicketStatusListRequestDto>, ITicketStatusAppService
{
    public const string InProgressNameStatusLower = "inprogress";
    public const string NewNameStatusLower = "new";
    public const string InCloseNameStatusLower = "answered";
    public const string InDraftNameStatusLower = "draft";
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<CustomerUserProfile, Guid> _customerUserProfileRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<TicketClientFavorite> _ticketClientFavoriteRepository;
    private readonly IRepository<TicketSpecialistFavorite> _ticketSpecialistFavoriteRepository;
    private readonly IRepository<Ticket, long> _ticketsRepository;

    public TicketStatusAppService(IReadOnlyRepository<TicketStatus, Guid> repository,
        IRepository<Ticket, long> ticketsRepository,
        IRepository<TicketClientFavorite> ticketClientFavoriteRepository,
        IRepository<TicketSpecialistFavorite> ticketSpecialistFavoriteRepository,
        IRepository<CustomerUserProfile, Guid> customerUserProfileRepository,
        IDataFilter dataFilter,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser) : base(repository)
    {
        this._ticketsRepository = ticketsRepository;
        this._ticketClientFavoriteRepository = ticketClientFavoriteRepository;
        this._ticketSpecialistFavoriteRepository = ticketSpecialistFavoriteRepository;
        this._customerUserProfileRepository = customerUserProfileRepository;
        this._dataFilter = dataFilter;
        this._currentTenant = currentTenant;
        this._currentUser = currentUser;
    }

    public async Task<List<TicketStatusDto>> GetListStatusesAsync()
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var statuses = await this.Repository.ToListAsync();
            var dtos = this.ObjectMapper.Map<List<TicketStatus>, List<TicketStatusDto>>(statuses);
            return dtos;
        }
    }

    public async Task<List<TicketStatusInfoDto>> GetTicketStatusByCreatorIdAndTenantAsync(Guid? creatorId)
    {
        var query = await this._ticketsRepository.GetQueryableAsync();
        var ticketStatuses = await this.Repository.ToListAsync();
        var ticketStatusInfos = new List<TicketStatusInfoDto>();
        foreach (var status in ticketStatuses)
        {
            long count = 0;
            if (status.IsPublic)
            {
                count = query
                    .Where(x => x.TicketStatusId == status.Id)
                    .LongCount();
            }
            else
            {
                count = query
                    .Where(x => x.TicketStatusId == status.Id)
                    .Where(x => x.CreatorId == creatorId)
                    .LongCount();
            }

            ticketStatusInfos.Add(new TicketStatusInfoDto
            {
                Id = status.Id,
                Count = count,
                Name = status.Name,
                DisplayName = status.DisplayName,
                DisplayNameMany = status.DisplayNameMany,
                Icon = status.Icon,
                Style = status.Style,
                IsPublic = status.IsPublic
            });
        }

        return ticketStatusInfos;
    }

    public async Task<TicketStatusInfoDto> GetMyTicketStatusByResponsibleIdAsync(Guid? responsibleId)
    {
        using (this._dataFilter.Enable<IMultiTenant>())
        {
            var myTicket = new TicketStatusInfoDto
            {
                Count = 0,
                Name = "InProgressNew",
                Icon = "fa fa-bookmark",
                Style = "color: #FFF; background-color: #7D8AA7",
                IsPublic = true
            };
            if (responsibleId == null || responsibleId == Guid.Empty)
            {
                return myTicket;
            }

            var query = await this._ticketsRepository.GetQueryableAsync();
            var ticketStatuses = await this.Repository.ToListAsync();
            var ticketClose = ticketStatuses.FirstOrDefault(x => x.Name.ToLowerInvariant() == InCloseNameStatusLower);

            if (this._currentTenant.Id == null)
            {
                myTicket.Count = query
                    .Where(x => x.ResponsibleId == responsibleId && x.TicketStatusId != ticketClose.Id)
                    .LongCount();
            }
            else
            {
                myTicket.Count = query
                    .Where(x => x.TenantId == this._currentTenant.Id)
                    .LongCount();
            }

            return myTicket;
        }
    }

    public async Task<TicketStatusInfoDto> GetTicketFavoriteInfoAsync()
    {
        var ticketStatusInfo = new TicketStatusInfoDto
        {
            Count = 0,
            Name = "InFavorite",
            Icon = "fas fa-star",
            Style = "color: #FFF; background-color: #008AA7",
            IsPublic = true
        };
        var currentUserId = this._currentUser?.Id ?? Guid.Empty;
        if (currentUserId == Guid.Empty)
        {
            return ticketStatusInfo;
        }

        if (this._currentTenant?.Id == null)
        {
            ticketStatusInfo.Count = await this._ticketSpecialistFavoriteRepository
                .CountAsync(x => x.SpecialistId == this._currentUser.Id);
        }
        else
        {
            var currentCustomerUserProfile =
                await this._customerUserProfileRepository.FirstOrDefaultAsync(x => x.IdentityUserId == currentUserId);
            if (currentCustomerUserProfile != null)
            {
                ticketStatusInfo.Count = await this._ticketClientFavoriteRepository
                    .CountAsync(x => x.ClientId == currentCustomerUserProfile.Id);
            }
        }

        return ticketStatusInfo;
    }

    public async Task<long> GetCountTicketByTicketStatusAsync(Guid? ticketStatusId)
    {
        using (this._dataFilter.Enable<IMultiTenant>())
        {
            if (ticketStatusId == null)
            {
                return await this._ticketsRepository.LongCountAsync();
            }

            var query = await this._ticketsRepository.GetQueryableAsync();
            query = query.Where(x => x.TicketStatusId == ticketStatusId);
            return query.LongCount();
        }
    }
}
