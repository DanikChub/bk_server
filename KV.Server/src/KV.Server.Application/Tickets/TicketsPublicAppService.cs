namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.Dtos.Employees;
using KV.Server.Dtos.File;
using KV.Server.Dtos.Tickets;
using KV.Server.File;
using KV.Server.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

[Authorize]
public class TicketsPublicAppService : ApplicationService, ITicketsPublicAppService
{
    public const string InProgressNameStatusLowerRus = "в работе";
    public const string NewNameStatusLowerRus = "новые";
    public const string InProgressNameStatusLower = "inprogress";
    public const string NewNameStatusLower = "new";
    public const string InProgressNewNameStatusLower = "inprogressnew";
    public const string InDraftNameStatusLower = "draft";
    public const string InFavoriteNameStatusLower = "infavorite";
    public const string InCloseNameStatusLower = "answered";
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IRepository<TicketHistory, Guid> _ticketHistoryRepository;
    private readonly IRepository<TicketStatus, Guid> _ticketStatusRepository;

    private readonly IRepository<ContractServicePackage> _contractServicePackageRepository;
    private readonly IRepository<ContractTicketsByTicketSectionByMonth, long> _contractTicketsByTicketSectionByMonth;
    private readonly IRepository<ContractTicketsByTicketTypeByMonth, long> _contractTicketsByTicketTypeByMonth;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<CustomerUserProfile, Guid> _customerUserProfileRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<TicketClientFavorite> _ticketClientFavoriteRepository;
    private readonly IRepository<TicketSpecialistFavorite> _ticketSpecialistFavoriteRepository;
    private readonly IRepository<Ticket, long> _ticketsRepository;
    private readonly IRepository<TicketTag> _ticketTagRepository;
    private readonly IRepository<TicketStatus, Guid> _ticketTicketStatusRepository;
    private readonly IRepository<TicketType, Guid> _ticketTypeRepository;

    public TicketsPublicAppService(IRepository<Ticket, long> ticketsRepository,
        IRepository<CustomerUserProfile, Guid> customerUserProfileRepository,
        IRepository<TicketStatus, Guid> ticketTicketStatusRepository,
        IRepository<ContractServicePackage> contractServicePackageRepository,
        IRepository<TicketTag> ticketTagRepository,
        IRepository<TicketClientFavorite> ticketClientFavoriteRepository,
        IRepository<TicketSpecialistFavorite> ticketSpecialistFavoriteRepository,
        IRepository<TicketType, Guid> ticketTypeRepository,
        IRepository<ContractTicketsByTicketTypeByMonth, long> contractTicketsByTicketTypeByMonth,
        IRepository<ContractTicketsByTicketSectionByMonth, long> contractTicketsByTicketSectionByMonth,
        IRepository<Contract, Guid> contractRepository,
        IDataFilter dataFilter,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IRepository<TicketHistory, Guid> ticketHistoryRepository,
        IRepository<TicketStatus, Guid> ticketStatusRepository)
    {
        this._ticketsRepository = ticketsRepository;
        this._customerUserProfileRepository = customerUserProfileRepository;
        this._ticketTicketStatusRepository = ticketTicketStatusRepository;
        this._contractServicePackageRepository = contractServicePackageRepository;
        this._ticketTagRepository = ticketTagRepository;
        this._ticketClientFavoriteRepository = ticketClientFavoriteRepository;
        this._ticketSpecialistFavoriteRepository = ticketSpecialistFavoriteRepository;
        this._ticketTypeRepository = ticketTypeRepository;
        this._contractTicketsByTicketTypeByMonth = contractTicketsByTicketTypeByMonth;
        this._contractTicketsByTicketSectionByMonth = contractTicketsByTicketSectionByMonth;
        this._contractRepository = contractRepository;
        this._dataFilter = dataFilter;
        this._currentTenant = currentTenant;
        this._currentUser = currentUser;
        _ticketHistoryRepository = ticketHistoryRepository;
        _ticketStatusRepository = ticketStatusRepository;
    }

    public async Task<PagedResultDto<TicketListItemDto>> GetTicketsListAsync(GetTicketsListRequestDto input)
    {
        var query = (await this._ticketsRepository.WithDetailsAsync(x => x.TicketStatus,
            x => x.Contract, x => x.Responsible))
            .AsQueryable();

        var currentUserFavoriteTicketIds = await this.GetFavoriteIdsTicketAsync();
        var currentCustomerUserProfile =
            await this._customerUserProfileRepository.FirstOrDefaultAsync(x => x.Id == this._currentUser.Id);

        if (!string.IsNullOrWhiteSpace(input.TicketStatusId))
        {
            var ticketStatuses = await this._ticketTicketStatusRepository.ToListAsync();
            var ticketInDraft = ticketStatuses.FirstOrDefault(x => x.Name.ToLowerInvariant() == InDraftNameStatusLower);
            var ticketClose = ticketStatuses.FirstOrDefault(x => x.Name.ToLowerInvariant() == InCloseNameStatusLower);
            if (input.TicketStatusId.ToLowerInvariant() == InProgressNewNameStatusLower)
            {
                if (this._currentTenant.Id == null)
                {
                    if (currentCustomerUserProfile != null)
                    {
                        query = query.Where(x =>
                            x.ResponsibleId == currentCustomerUserProfile.Id && x.TicketStatusId != ticketClose.Id);
                    }
                }
                else
                {
                    query = query.Where(x => x.TenantId == this._currentTenant.Id);
                }
            }
            else if (input.TicketStatusId.ToLowerInvariant() == InFavoriteNameStatusLower)
            {
                query = query.Where(x => currentUserFavoriteTicketIds.Contains(x.Id));
            }
            else if (input.TicketStatusId.ToLowerInvariant() == ticketInDraft?.Id.ToString())
            {
                query = query.Where(x => x.TicketStatusId == Guid.Parse(input.TicketStatusId));
                query = query.Where(x => x.CreatorId == this._currentUser.Id);
            }
            else
            {
                query = query.Where(x => x.TicketStatusId == Guid.Parse(input.TicketStatusId));
            }
        }

        if (!string.IsNullOrWhiteSpace(input.SearchSubject))
        {
            long ticketId = -1;
            if (long.TryParse(input.SearchSubject.Trim(), out var result))
            {
                ticketId = result;
            }

            query = query.Where(x =>
                x.Subject.ToLowerInvariant().Contains(input.SearchSubject.Trim().ToLowerInvariant()) || x.Id == ticketId);
        }

        if (!string.IsNullOrWhiteSpace(input.SearchCreationTime))
        {
            query = query.Where(x => x.CreationTime > DateTime.Parse(input.SearchCreationTime));
        }

        if (!string.IsNullOrWhiteSpace(input.SearchTicketStatusId))
        {
            query = query.Where(x => x.TicketStatusId == Guid.Parse(input.SearchTicketStatusId));
        }

        if (!string.IsNullOrWhiteSpace(input.ServicePackageId))
        {
            query = query.Where(x =>
                x.Contract.ServicePackages.FirstOrDefault(x =>
                    x.ServicePackageId == Guid.Parse(input.ServicePackageId)) != null);
        }

        if (!string.IsNullOrEmpty(input.Sorting))
        {
            if (input.Sorting == "subject asc")
            {
                query = query.OrderBy(x => x.Subject);
            }
            else if (input.Sorting == "creationTime asc")
            {
                query = query.OrderBy(x => x.CreationTime);
            }
            else if (input.Sorting == "id asc")
            {
                query = query.OrderBy(x => x.Id);
            }
            else if (input.Sorting == "servicePackageName asc")
            {
                query = query.OrderBy(x => x.Contract.ServicePackages.FirstOrDefault().ServicePackage.Name);
            }
            else if (input.Sorting == "ticketStatusDisplayName asc")
            {
                query = query.OrderBy(x => x.TicketStatus.DisplayName);
            }
            else if (input.Sorting == "subject desc")
            {
                query = query.OrderByDescending(x => x.Subject);
            }
            else if (input.Sorting == "creationTime desc")
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }
            else if (input.Sorting == "id desc")
            {
                query = query.OrderByDescending(x => x.Id);
            }
            else if (input.Sorting == "servicePackageName desc")
            {
                query = query.OrderByDescending(x => x.Contract.ServicePackages.FirstOrDefault().ServicePackage.Name);
            }
            else if (input.Sorting == "ticketStatusDisplayName desc")
            {
                query = query.OrderByDescending(x => x.TicketStatus.DisplayName);
            }
            else
            {
                query = query.OrderBy(input.Sorting.Replace("code", "id"));
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.CreationTime);
        }

        var totalCount = query.Count();
        var tickets = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount == 0 ? GetTicketsListRequestDto.DefaultPageSize : input.MaxResultCount)
            .ToList();
        var ticketsDtos = this.ObjectMapper.Map<List<Ticket>, List<TicketListItemDto>>(tickets);
        for (var i = 0; i < tickets.Count; i++)
        {
            var ticket = tickets[i];
            var ticketDto = ticketsDtos[i];
        }

        return new PagedResultDto<TicketListItemDto>(totalCount, ticketsDtos);
    }

    private async Task<List<long>> GetFavoriteIdsTicketAsync()
    {
        if (this._currentTenant?.Id == null)
        {
            var favorites = (await this._ticketSpecialistFavoriteRepository
                .GetQueryableAsync())
                .Where(x => x.SpecialistId == this._currentUser.Id)
                .Select(x => x.TicketId)
                .ToList();
            return favorites;
        }
        else
        {
            var currentCustomerUserProfile =
                await this._customerUserProfileRepository.FirstOrDefaultAsync(x => x.IdentityUserId == this._currentUser.Id);
            var currentCustomerUserProfileId = currentCustomerUserProfile?.Id ?? Guid.Empty;
            var favorites = (await this._ticketClientFavoriteRepository
                .GetQueryableAsync())
                .Where(x => x.ClientId == currentCustomerUserProfileId)
                .Select(x => x.TicketId)
                .ToList();
            return favorites;
        }
    }

    [Authorize]
    public async Task<TicketDetailsDto> GetTicketDetailsAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = (await this._ticketsRepository.WithDetailsAsync(
                x => x.Creator,
                x => x.TenantProfile,
                x => x.Responsible,
                x => x.Responsible.IdentityUser,
                x => x.TicketStatus,
                x => x.TicketType)).FirstOrDefault(x => x.Id == ticketId);
            if (ticket == null)
            {
                return null;
            }

            var ticketDetailsDto = this.ObjectMapper.Map<Ticket, TicketDetailsDto>(ticket);
            var customerUserProfile = await this._customerUserProfileRepository
                .FirstOrDefaultAsync(x => x.IdentityUserId == ticket.CreatorId);
            ticketDetailsDto.CreatorJobPost = customerUserProfile?.JobPost;
            ticketDetailsDto.CreatorMiddleName = customerUserProfile?.MiddleName;
            var servicePackages = ((await this._contractServicePackageRepository
                        .WithDetailsAsync(x => x.ServicePackage))
                    .Where(x => x.ContractId == ticket.ContractId)
                    .ToList())
                .Select(x => x.ServicePackage)
                .ToList();

            ticketDetailsDto.ServicePackages = ticket.ServicePackages;

            return ticketDetailsDto;
        }
    }

    [Authorize]
    public async Task<TicketRatingDto> GetTicketRatingAsync(long ticketId)
    {
        var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
        var dto = new TicketRatingDto
        {
            Rating = ticket.Rating
        };
        return dto;
    }

    public async Task UpdateTicketDraftAsync(long id, CreateUpdateTicketDto dto)
    {
        using (this._dataFilter.Enable<IMultiTenant>())
        {
            var draftTicketStatus =
                await this._ticketTicketStatusRepository.FirstOrDefaultAsync(x =>
                    x.Name.ToLowerInvariant() == InDraftNameStatusLower);
            if (dto.TicketStatusId == draftTicketStatus.Id)
            {
                var ticketNew =
                    await this._ticketTicketStatusRepository.FirstOrDefaultAsync(x =>
                        x.Name.ToLowerInvariant() == NewNameStatusLower);
                var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == id);
                if (ticket != null)
                {
                    var statuses = (await _ticketStatusRepository.WithDetailsAsync());
                    var status = statuses.FirstOrDefault(x => x.Id == ticket.TicketStatusId);
                    var ticketType = await this._ticketTypeRepository.FirstOrDefaultAsync(x => x.Id == dto.TicketTypeId);
                    ticket = this.ObjectMapper.Map(dto, ticket);
                    ticket.CreatorId = dto.CreatorId;
                    ticket.UpdateTicketSection(dto.TicketSectionId);
                    ticket.SetDeadLine(this.Clock.Now.Add(ticketType.SLATime));
                    ticket.UpdateStatus(ticketNew.Id, $"Статус заявки изменён на \"{status.DisplayName}\"", TicketHistoryType.StatusUpdate);
                    if (ticket.ContractId == Guid.Empty)
                    {
                        var tenantId = this._currentTenant?.Id;
                        if (tenantId == null)
                        {
                            ticket.ContractId = null;
                        }
                        else
                        {
                            var contract = (await this._contractRepository
                                    .GetQueryableAsync())
                                .OrderByDescending(x => x.ContractFinishDate)
                                .FirstOrDefault(x => x.TenantId == tenantId);
                            if (contract == null || contract.ContractFinishDate < this.Clock.Now)
                            {
                                throw new UserFriendlyException("Нет действующих контрактов.");
                            }

                            await this.CreateTicketTypeByMonthAsync(contract.Id, dto.TicketTypeId);
                            if (dto.TicketSectionId != null)
                            {
                                await this.CreateTicketSectionByMonthAsync(contract.Id, dto.TicketSectionId ?? Guid.Empty);
                            }

                            ticket.ContractId = contract.Id;
                        }
                    }

                    await this._ticketsRepository.UpdateAsync(ticket);
                    await this.CurrentUnitOfWork.SaveChangesAsync();
                }
            }
        }
    }

    [Authorize]
    public async Task<TicketDetailsDto> CreateTicketAsync(CreateUpdateTicketDto dto)
    {

        var ticketStatus = await this._ticketTicketStatusRepository.FirstOrDefaultAsync(x => x.Id == dto.TicketStatusId);
        var ticket = new Ticket(dto.Subject, dto.Description, dto.TicketTypeId, ticketStatus.Id, null);
        var ticketType = await this._ticketTypeRepository.FirstOrDefaultAsync(x => x.Id == dto.TicketTypeId);
        ticket.CreatorId = dto.CreatorId;
        ticket.UpdateTicketSection(dto.TicketSectionId);
        ticket.SetDeadLine(this.Clock.Now.Add(ticketType.SLATime));
        if (dto.ContractId == Guid.Empty)
        {
            var tenantId = this._currentTenant?.Id;
            if (tenantId == null)
            {
                ticket.ContractId = null;
            }
            else
            {
                var contract = (await this._contractRepository
                    .GetQueryableAsync())
                    .OrderByDescending(x => x.ContractFinishDate)
                    .FirstOrDefault(x => x.TenantId == tenantId);
                if (contract == null || contract.ContractFinishDate < this.Clock.Now)
                {
                    throw new UserFriendlyException("Нет действующих контрактов.");
                }

                if (ticketStatus.IsPublic)
                {
                    await this.CreateTicketTypeByMonthAsync(contract.Id, dto.TicketTypeId);
                    if (dto.TicketSectionId != null)
                    {
                        await this.CreateTicketSectionByMonthAsync(contract.Id, dto.TicketSectionId ?? Guid.Empty);
                    }
                }

                ticket.ContractId = contract.Id;
            }
        }
        else
        {
            ticket.ContractId = dto.ContractId;
        }

        await this._ticketsRepository.InsertAsync(ticket);
        await this.CurrentUnitOfWork.SaveChangesAsync();
        var uploadedFiles = this.ObjectMapper.Map<List<UploadedFileDto>, List<UploadedFile>>(dto.Attachments);
        var ticketDto = this.ObjectMapper.Map<Ticket, TicketDetailsDto>(ticket);
        return ticketDto;

    }

    public async Task MakeReadTicketByIdAsync(long ticketId, UpdateReadTicketDto dto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
            ticket.ReadByClientDate = dto.ReadByClientDate;
            ticket.ReadBySpecialistDate = dto.ReadBySpecialistDate;
            await this._ticketsRepository.UpdateAsync(ticket);
        }
    }

    [Authorize]
    public async Task<EmployeeProfileDto> GetSpecialistByTicketIdAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var specialist = (await this._ticketsRepository
                    .WithDetailsAsync(x => x.Responsible))
                .Where(x => x.Id == ticketId)
                .Select(x => x.Responsible)
                .FirstOrDefault();

            return this.ObjectMapper.Map<EmployeeProfile, EmployeeProfileDto>(specialist);
        }
    }

    [Authorize]
    public async Task SetRatingTicketAsync(long ticketId, CreateUpdateTicketRatingDto dto)
    {
        var currentUserId = this._currentUser?.Id;
        var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
        ticket.SetRating(dto.Rating);
        await this._ticketsRepository.UpdateAsync(ticket);
    }

    private async Task CreateTicketTypeByMonthAsync(Guid contractId, Guid ticketTypeId)
    {
        var ticketTypeByMonth = await this._contractTicketsByTicketTypeByMonth.FirstOrDefaultAsync(x =>
            x.ContractId == contractId &&
            x.TicketTypeId == ticketTypeId &&
            x.Year == this.Clock.Now.Year &&
            x.Month == this.Clock.Now.Month);
        if (ticketTypeByMonth == null)
        {
            await this._contractTicketsByTicketTypeByMonth.InsertAsync(new ContractTicketsByTicketTypeByMonth
            {
                ContractId = contractId,
                TicketTypeId = ticketTypeId,
                Year = this.Clock.Now.Year,
                Month = this.Clock.Now.Month,
                Count = 1
            });
        }
        else
        {
            ticketTypeByMonth.Count += 1;
            await this._contractTicketsByTicketTypeByMonth.UpdateAsync(ticketTypeByMonth);
        }
    }

    private async Task CreateTicketSectionByMonthAsync(Guid contractId, Guid ticketSectionId)
    {
        var ticketSectionByMonth = await this._contractTicketsByTicketSectionByMonth.FirstOrDefaultAsync(x =>
            x.ContractId == contractId &&
            x.TicketSectionId == ticketSectionId &&
            x.Year == this.Clock.Now.Year &&
            x.Month == this.Clock.Now.Month);
        if (ticketSectionByMonth == null)
        {
            await this._contractTicketsByTicketSectionByMonth.InsertAsync(new ContractTicketsByTicketSectionByMonth
            {
                ContractId = contractId,
                TicketSectionId = ticketSectionId,
                Year = this.Clock.Now.Year,
                Month = this.Clock.Now.Month,
                Count = 1
            });
        }
        else
        {
            ticketSectionByMonth.Count += 1;
            await this._contractTicketsByTicketSectionByMonth.UpdateAsync(ticketSectionByMonth);
        }
    }

    public async Task<TicketHistoryDto> CreateTicketHistoryAsync(CreateTicketHistoryDto dto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == dto.TicketId);
            var statuses = (await _ticketStatusRepository.WithDetailsAsync());
            var status = statuses.FirstOrDefault(x => x.Id == ticket.TicketStatusId);
            if (status.Name == TicketStatusConstants.Answered)
            {
                ticket.UpdateStatus(statuses.Where(x => x.Name == TicketStatusConstants.Renewed).FirstOrDefault().Id, 
                    $"Статус заявки изменён на \"{status.DisplayName}\"", 
                    TicketHistoryType.StatusUpdate);
            }
            var responsibleId = ticket?.ResponsibleId ?? Guid.Empty;
            var responsible = await this._customerUserProfileRepository.FirstOrDefaultAsync(x => x.Id == responsibleId);
            var type = responsible?.IdentityUserId == dto.CreatorId
                ? TicketHistoryType.SpecialistMessage
                : TicketHistoryType.CustomerMessage;
            var ticketHistory =
                new TicketHistory(dto.CreatorId, dto.Description, dto.TicketId, dto.TicketStatusId, type);
            await this._ticketHistoryRepository.InsertAsync(ticketHistory);
            await this.CurrentUnitOfWork.SaveChangesAsync();
            var uploadedFiles = this.ObjectMapper.Map<List<UploadedFileDto>, List<UploadedFile>>(dto.Attachments);

            await this._ticketsRepository.UpdateAsync(ticket, true);
            return this.ObjectMapper.Map<TicketHistory, TicketHistoryDto>(ticketHistory);
        }
    }
}
