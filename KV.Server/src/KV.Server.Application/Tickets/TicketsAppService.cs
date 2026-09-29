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
using KV.Server.Interfaces;
using KV.Server.Permissions;
using KV.Server.Permissions.Tickets;
using KV.Server.Profiles;
using KV.Server.Tags;
using KV.Server.TicketHoursSpent;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

[Authorize(TicketPermissions.Tickets.Default)]
public class TicketsAppService : ApplicationService, ITicketsAppService
{
    public const string InProgressNameStatusLowerRus = "в работе";
    public const string NewNameStatusLowerRus = "новые";
    public const string InProgressNameStatusLower = "inprogress";
    public const string NewNameStatusLower = "new";
    public const string InProgressNewNameStatusLower = "inprogressnew";
    public const string InFavoriteNameStatusLower = "infavorite";
    public const string InDraftNameStatusLower = "draft";
    public const string InCloseNameStatusLower = "answered";
    private readonly IRepository<ContractServicePackage> _contractServicePackageRepository;
    private readonly IRepository<ConstraintType, Guid> _constraintTypeRepository;
    private readonly IRepository<ContractTicketsByTicketSectionByMonth, long> _contractTicketsByTicketSectionByMonth;
    private readonly IRepository<ContractTicketsByTicketTypeByMonth, long> _contractTicketsByTicketTypeByMonth;
    private readonly IRepository<EmployeeProfile, Guid> _employeeProfileRepository;
    private readonly IWorkCalendarAppService _workCalendarAppService;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Tag, Guid> _tagRepository;
    private readonly IRepository<TenantProfileManager> _tenantProfileManagerRepository;
    private readonly IRepository<TenantProfile, Guid> _tenantProfileRepository;
    private readonly IRepository<TicketHistory, Guid> _ticketHistoryRepository;
    private readonly IRepository<CustomerUserProfile, Guid> _customerUserProfileRepository;
    private readonly IRepository<Ticket, long> _ticketsRepository;
    private readonly IRepository<TicketTag> _ticketTagRepository;
    private readonly IRepository<TicketStatus, Guid> _ticketStatusRepository;
    private readonly IRepository<TicketType, Guid> _ticketTypeRepository;
    private readonly IRepository<ContractSetting, Guid> _contractSettingRepository;
    private readonly IRepository<TicketHoursSpentHistory, Guid> _ticketHoursSpentHistoryRepository;
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IRepository<TicketSpecialistFavorite> _ticketSpecialistFavoriteRepository;

    public TicketsAppService(IRepository<Ticket, long> ticketsRepository,
        IRepository<TicketHistory, Guid> ticketHistoryRepository,
        IRepository<CustomerUserProfile, Guid> customerUserProfileRepository,
        IRepository<TicketStatus, Guid> ticketStatusRepository,
        IRepository<TenantProfile, Guid> tenantProfileRepository,
        IRepository<TenantProfileManager> tenantProfileManagerRepository,
        IRepository<ContractServicePackage> contractServicePackageRepository,
        IRepository<TicketTag> ticketTagRepository,
        IRepository<Tag, Guid> tagRepository,
        IRepository<TicketType, Guid> ticketTypeRepository,
        IRepository<ContractTicketsByTicketTypeByMonth, long> contractTicketsByTicketTypeByMonth,
        IRepository<ContractTicketsByTicketSectionByMonth, long> contractTicketsByTicketSectionByMonth,
        IRepository<ContractSetting, Guid> contractSettingRepository,
        IRepository<TicketHoursSpentHistory, Guid> ticketHoursSpentHistoryRepository,
        IRepository<Contract, Guid> contractRepository,
        IRepository<TicketSpecialistFavorite> ticketSpecialistFavoriteRepository,
        IDataFilter dataFilter,
        IRepository<ConstraintType, Guid> constraintTypeRepository,
        IRepository<EmployeeProfile, Guid> employeeProfileRepository,
        IWorkCalendarAppService workCalendarAppService)
    {
        this._ticketsRepository = ticketsRepository;
        this._ticketHistoryRepository = ticketHistoryRepository;
        _customerUserProfileRepository = customerUserProfileRepository;
        this._ticketStatusRepository = ticketStatusRepository;
        this._tenantProfileRepository = tenantProfileRepository;
        this._tenantProfileManagerRepository = tenantProfileManagerRepository;
        this._contractServicePackageRepository = contractServicePackageRepository;
        this._ticketTagRepository = ticketTagRepository;
        this._tagRepository = tagRepository;
        this._ticketTypeRepository = ticketTypeRepository;
        this._contractTicketsByTicketTypeByMonth = contractTicketsByTicketTypeByMonth;
        this._contractTicketsByTicketSectionByMonth = contractTicketsByTicketSectionByMonth;
        this._contractSettingRepository = contractSettingRepository;
        this._ticketHoursSpentHistoryRepository = ticketHoursSpentHistoryRepository;
        this._contractRepository = contractRepository;
        this._ticketSpecialistFavoriteRepository = ticketSpecialistFavoriteRepository;
        this._dataFilter = dataFilter;
        _constraintTypeRepository = constraintTypeRepository;
        _employeeProfileRepository = employeeProfileRepository;
        _workCalendarAppService = workCalendarAppService;
    }

    [Authorize(TicketPermissions.Tickets.Get)]
    public async Task<PagedResultDto<TicketListItemDto>> GetTicketsListAsync(GetTicketsListRequestDto input)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = (await this._ticketsRepository
                .WithDetailsAsync(x => x.Contract,
                    x => x.TicketStatus, x => x.TicketTags, x => x.TenantProfile, x => x.Responsible, x => x.TicketType, x => x.TicketSection))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(input.TenantId))
            {
                query = query.Where(x => x.TenantId == input.TenantId.To<Guid>());
            }

            if (!string.IsNullOrWhiteSpace(input.IdentityUserId))
            {
                query = query.Where(x => x.CreatorId == input.IdentityUserId.To<Guid>());
            }

            if (!string.IsNullOrWhiteSpace(input.ResponsibleId))
            {
                query = query.Where(x => x.ResponsibleId == input.ResponsibleId.To<Guid>());
            }

            if (!string.IsNullOrWhiteSpace(input.ManagerId))
            {
                var tenantProfiles = ((await this._tenantProfileRepository.GetQueryableAsync())
                        .Where(x => x.ResponsibleManagerId == Guid.Parse(input.ManagerId))
                        .Select(x => x.Id)
                        .ToList())
                    .Cast<Guid?>()
                    .ToList();

                var tenantProfileManagers = (await this._tenantProfileManagerRepository.GetQueryableAsync())
                        .Where(x => x.ManagerId == Guid.Parse(input.ManagerId))
                        .Select(x => x.TenantProfileId)
                        .ToList()
                    .Cast<Guid?>()
                    .ToList();

                query = query.Where(x => tenantProfiles.Contains(x.TenantId)
                                         || tenantProfileManagers.Contains(x.TenantId));
            }

            var creatorName = string.Empty;
            var creatorSurname = string.Empty;
            if (!string.IsNullOrWhiteSpace(input.SearchCreatorFullName))
            {
                var splitCreatorFullName = input.SearchCreatorFullName.Replace(".", "").Split(' ');
                creatorSurname = splitCreatorFullName.FirstOrDefault();
                creatorName = splitCreatorFullName.LastOrDefault();
            }

            var responsibleLastName = string.Empty;
            var responsibleFirstNameOrMiddleName = string.Empty;
            var responsibleMiddleName = string.Empty;
            if (!string.IsNullOrWhiteSpace(input.SearchResponsibleFullName))
            {
                var splitResponsibleFullName = input.SearchResponsibleFullName.Replace(".", "").Split(' ');
                responsibleLastName = splitResponsibleFullName.FirstOrDefault();
                if (splitResponsibleFullName.Length >= 2)
                {
                    responsibleFirstNameOrMiddleName = splitResponsibleFullName[1];
                }

                if (splitResponsibleFullName.Length >= 3)
                {
                    responsibleMiddleName = splitResponsibleFullName.LastOrDefault();
                }
            }

            if (!string.IsNullOrWhiteSpace(input.SearchStr))
            {
                long ticketId = -1;
                if (long.TryParse(input.SearchStr.Trim(), out var result))
                {
                    ticketId = result;
                }

                query = query.Where(x =>
                    x.TenantProfile.ShortName.ToLower().Contains(input.SearchStr.ToLower().Trim()) || x.Id == ticketId);
            }
            if (!string.IsNullOrWhiteSpace(input.SearchResponsibleFullName))
            {
                var lowerInvariantFullName = input.SearchResponsibleFullName.Trim().ToLower();

                query = query.Where(x =>
                    x.Responsible.LastName.ToLower().Contains(lowerInvariantFullName.Replace(".", string.Empty))
                );
            }
            if (!string.IsNullOrWhiteSpace(input.SearchCreatorFullName))
            {
                query = query.Where(x =>
                    x.Creator.Surname.ToLowerInvariant().Contains(input.SearchCreatorFullName.Trim().ToLowerInvariant())
                    || x.Creator.Name.Contains(input.SearchCreatorFullName.Trim().ToLowerInvariant())
                    || (creatorName != string.Empty && x.Creator.Name.ToLowerInvariant().Contains(creatorName.Trim().ToLower())
                                                    && creatorSurname != string.Empty &&
                                                    x.Creator.Surname.ToLowerInvariant()
                                                        .Contains(creatorSurname.Trim().ToLowerInvariant())));
            }


            else if (!string.IsNullOrWhiteSpace(input.SearchSubject))
            {
                query = query.Where(x => x.Subject.ToLower().Contains(input.SearchSubject.Trim().ToLower()));
            }

            else if (!string.IsNullOrWhiteSpace(input.SearchCustomerLongName))
            {
                query = query.Where(x =>
                    x.TenantProfile.LongName.ToLower().Contains(input.SearchCustomerLongName.Trim().ToLower()));
            }

            else if (!string.IsNullOrWhiteSpace(input.SearchCreationTime))
            {
                query = query.Where(x => x.CreationTime >= DateTime.Parse(input.SearchCreationTime));
            }

            else if (!string.IsNullOrWhiteSpace(input.SearchTicketId))
            {
                query = query.Where(x => x.Id == long.Parse(input.SearchTicketId));
            }

            else if (!string.IsNullOrWhiteSpace(input.SearchTicketTypeId))
            {
                query = query.Where(x => x.TicketTypeId == Guid.Parse(input.SearchTicketTypeId));
            }

            else if (!string.IsNullOrWhiteSpace(input.SearchTagId))
            {
                query = query.Where(x => x.TicketTags.Any(x => x.TagId == Guid.Parse(input.SearchTagId)));
            }

            else if (!string.IsNullOrWhiteSpace(input.SearchTicketSectionId))
            {
                query = query.Where(x => x.TicketSectionId == Guid.Parse(input.SearchTicketSectionId));
            }

            else if (!string.IsNullOrWhiteSpace(input.SearchDueDate))
            {
                query = query.Where(x => x.DueDate >= DateTime.Parse(input.SearchDueDate));
            }

            else if (!string.IsNullOrWhiteSpace(input.SearchTicketStatusId))
            {
                var searchGuid = Guid.Empty;
                if (Guid.TryParse(input.SearchTicketStatusId, out var parsedGuid))
                {
                    searchGuid = parsedGuid;
                }

                var employee = (await _employeeProfileRepository.GetQueryableAsync())
                    .FirstOrDefault(x => x.IdentityUserId == searchGuid);
                var currentUserFavoriteTicketIds = (await this._ticketSpecialistFavoriteRepository
                    .GetQueryableAsync())
                    .Where(x => x.SpecialistId == (employee != null ? employee.Id : CurrentUser.Id))
                    .Select(x => x.TicketId)
                    .ToList();
                if (input.SearchTicketStatusId.ToLower() == InFavoriteNameStatusLower)
                {
                    query = query.Where(x => currentUserFavoriteTicketIds.Contains(x.Id));
                }
                else if (employee != null)
                {
                    query = query.Where(x => x.ResponsibleId == employee.Id);
                }
                else
                {
                    query = query.Where(x => x.TicketStatusId == Guid.Parse(input.SearchTicketStatusId));
                }
            }

            if (!string.IsNullOrWhiteSpace(input.ServicePackageId))
            {
                query = query.Where(x =>
                    x.Contract.ServicePackages.FirstOrDefault(x =>
                        x.ServicePackageId == Guid.Parse(input.ServicePackageId)) != null);
            }

            if (!string.IsNullOrEmpty(input.Sorting))
            {
                if (input.Sorting == "responsibleFullName asc")
                {
                    query = query.OrderBy(x => x.Responsible.LastName);
                }
                else if (input.Sorting == "responsibleLastName asc")
                {
                    query = query.OrderBy(x => x.Responsible.LastName);
                }
                else if (input.Sorting == "creatorFullName asc")
                {
                    query = query.OrderBy(x => x.Creator.Surname);
                }
                else if (input.Sorting == "ticketStatusDisplayName asc")
                {
                    query = query.OrderBy(x => x.TicketStatus.Name);
                }
                else if (input.Sorting == "ticketTypeName asc")
                {
                    query = query.OrderBy(x => x.TicketType.Name);
                }
                else if (input.Sorting == "ticketSectionName asc")
                {
                    query = query.OrderBy(x => x.TicketSection.Name);
                }
                else if (input.Sorting == "customerShortName asc")
                {
                    query = query.OrderBy(x => x.TenantProfile.ShortName);
                }
                else if (input.Sorting == "responsibleFullName desc")
                {
                    query = query.OrderByDescending(x => x.Responsible.LastName);
                }
                else if (input.Sorting == "responsibleLastName desc")
                {
                    query = query.OrderBy(x => x.Responsible.LastName);
                }
                else if (input.Sorting == "creatorFullName desc")
                {
                    query = query.OrderByDescending(x => x.Creator.Surname);
                }
                else if (input.Sorting == "ticketStatusDisplayName desc")
                {
                    query = query.OrderByDescending(x => x.TicketStatus.Name);
                }
                else if (input.Sorting == "ticketTypeName desc")
                {
                    query = query.OrderByDescending(x => x.TicketType.Name);
                }
                else if (input.Sorting == "ticketSectionName desc")
                {
                    query = query.OrderByDescending(x => x.TicketSection.Name);
                }
                else if (input.Sorting == "customerShortName desc")
                {
                    query = query.OrderByDescending(x => x.TenantProfile.ShortName);
                }
                else if (input.Sorting == "lastTagName asc")
                {
                    query = query.OrderBy(x => x.TicketTags.FirstOrDefault());
                }
                else if (input.Sorting == "lastTagName desc")
                {
                    query = query.OrderByDescending(x => x.TicketTags.FirstOrDefault());
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
            var tags = await this._tagRepository.ToListAsync();
            for (var i = 0; i < tickets.Count; i++)
            {
                var ticket = tickets[i];
                var ticketDto = ticketsDtos[i];
                ticketDto.LastTagName =
                    tags.FirstOrDefault(x => x.Id == ticket.TicketTags.FirstOrDefault()?.TagId)?.Name ?? string.Empty;
            }

            return new PagedResultDto<TicketListItemDto>(totalCount, ticketsDtos);
        }
    }

    [Authorize(TicketPermissions.Tickets.Delete)]
    public async Task DeleteTicketAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            await this._ticketsRepository.DeleteAsync(ticketId);
        }
    }
    [Authorize(TicketPermissions.Tickets.Delete)]
    public async Task DeleteTicketHistoryRecordByIdAsync(Guid ticketHistoryId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            await this._ticketHistoryRepository.DeleteAsync(ticketHistoryId);
        }
    }
    [Authorize(TicketPermissions.Tickets.Edit)]
    public async Task UpdateTicketAsync(long ticketId, CreateUpdateTicketDto dto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
            var ticketType = await this._ticketTypeRepository.FirstOrDefaultAsync(x => x.Id == dto.TicketTypeId);

            var holidays = await _workCalendarAppService.GetHolidaysAsync(Clock.Now, Clock.Now.AddDays(30));

            var deadline = CalculateDeadline(Clock.Now, ticketType.SLATime, holidays);

            ticket.SetDeadLine(deadline);
            ticket = this.ObjectMapper.Map(dto, ticket);
            await this._ticketsRepository.UpdateAsync(ticket, true);
        }
    }
    [Authorize(TicketPermissions.Tickets.Edit)]
    public async Task UpdateTicketDescriptionAsync(long ticketId, UpdateTicketDescriptionDto dto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
            ticket.SetDescription(dto.Description);
            await this._ticketsRepository.UpdateAsync(ticket, true);
        }
    }
    [Authorize(TicketPermissions.Tickets.Edit)]
    public async Task UpdateTicketTypeByTicketIdAsync(long ticketId, Guid ticketTypeId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
            var oldTicketTypeId = ticket.TicketTypeId;
            var ticketType = await this._ticketTypeRepository.FirstOrDefaultAsync(x => x.Id == ticketTypeId);

            var holidays = await _workCalendarAppService.GetHolidaysAsync(Clock.Now, Clock.Now.AddDays(30));

            var deadline = CalculateDeadline(Clock.Now, ticketType.SLATime, holidays);
            ticket.SetDeadLine(deadline);
            ticket.UpdateTicketType(ticketTypeId);
            await this._ticketsRepository.UpdateAsync(ticket, true);
            var contractId = ticket?.ContractId;
            if (contractId == null)
            {
                var contract = (await this._contractRepository.GetQueryableAsync())
                    .Where(x => x.TenantId == ticket.TenantId)
                    .OrderByDescending(x => x.ContractFinishDate)
                    .FirstOrDefault();
                if (contract?.ContractFinishDate > this.Clock.Now)
                {
                    contractId = contract?.Id;
                }
            }
            await this.CreateTicketTypeByMonthAsync(contractId, oldTicketTypeId, ticketTypeId);
        }
    }

    [Authorize(TicketPermissions.Tickets.Edit)]
    public async Task UpdateTicketSectionByTicketIdAsync(long ticketId, Guid? ticketSectionId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
            var oldTicketSectionId = ticket.TicketSectionId;
            ticket.UpdateTicketSection(ticketSectionId);
            await this._ticketsRepository.UpdateAsync(ticket, true);
            var contractId = ticket?.ContractId;
            if (contractId == null)
            {
                var contract = (await this._contractRepository.GetQueryableAsync())
                    .Where(x => x.TenantId == ticket.TenantId)
                    .OrderByDescending(x => x.ContractFinishDate)
                    .FirstOrDefault();
                if (contract?.ContractFinishDate > this.Clock.Now)
                {
                    contractId = contract?.Id;
                }
            }
            await this.CreateTicketSectionByMonthAsync(contractId, oldTicketSectionId, ticketSectionId);
        }
    }

    [Authorize(TicketPermissions.Tickets.Edit)]
    public async Task UpdateTicketStatusByTicketIdAsync(long ticketId, Guid ticketStatusId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var status = await _ticketStatusRepository.GetAsync(ticketStatusId);
            var tickerQueryable = await _ticketsRepository.WithDetailsAsync(x => x.TicketHistory);
            var ticket = tickerQueryable.FirstOrDefault(x => x.Id == ticketId);
            ticket.UpdateStatus(ticketStatusId, $"Статус заявки изменён на \"{status.DisplayName}\"", TicketHistoryType.StatusUpdate);
            await this._ticketsRepository.UpdateAsync(ticket, true);
        }
    }

    [Authorize(TicketPermissions.Tickets.Edit)]
    public async Task UpdateTicketConstraintByTicketIdAsync(long ticketId, Guid constraintTypeId, int value)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = (await this._ticketsRepository.WithDetailsAsync(x => x.Contract)).FirstOrDefault(x => x.Id == ticketId);


            var contractId = ticket?.ContractId;
            // Проверка на существование контракта
            if (contractId == null)
            {
                throw new Volo.Abp.UserFriendlyException("Нет действующего договора");
            }
            if (contractId != null && constraintTypeId != Guid.Empty)
            {
                var ticketHistory = new TicketHistory(this.CurrentUser?.Id, string.Empty, ticketId, ticket.TicketStatusId, TicketHistoryType.SpecialistMessage);

                var ticketHistoryNew = await this._ticketHistoryRepository.InsertAsync(ticketHistory);

                var contrainType = await _constraintTypeRepository.GetAsync(constraintTypeId);
                if (value != 0)
                {
                    var ticketHistoryChangeLimit = new TicketHistory(this.CurrentUser?.Id, $"Установлено ограничение типа \"{contrainType.Name}\" в размере {value}", ticketId, ticket.TicketStatusId, TicketHistoryType.ChangeLimit);
                    var ticketHistoryChangeLimitNew = await this._ticketHistoryRepository.InsertAsync(ticketHistoryChangeLimit);
                }
                var ticketHoursSpentHistory = new TicketHoursSpentHistory(value, constraintTypeId, ticketHistoryNew.Id);
                var ticketHoursSpentHistoryNew = await this._ticketHoursSpentHistoryRepository.InsertAsync(ticketHoursSpentHistory);

                await this.CurrentUnitOfWork.SaveChangesAsync();
            }
        }
    }

    [Authorize(SpecialistPermissions.Profiles.Create)]
    public async Task<EmployeeProfileDto> AssignedSpecialistAsync(long ticketId, Guid? employeeProfileDtoId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
            var employee =
                await this._employeeProfileRepository.FirstOrDefaultAsync(x => x.Id == employeeProfileDtoId);
            if (ticket?.ResponsibleId == null && employee?.Id == null)
            {
                return null;
            }

            if (ticket.ResponsibleId == employeeProfileDtoId)
            {
                return this.ObjectMapper.Map<EmployeeProfile, EmployeeProfileDto>(employee);
            }

            var ticketStatus =
                await this._ticketStatusRepository.FirstOrDefaultAsync(x =>
                    x.Name.ToLower() == InProgressNameStatusLower);
            var customerSurname =
                $"{employee?.LastName ?? employee?.IdentityUser?.Surname ?? employee?.IdentityUser?.UserName}";
            var description = "";
            if (employee == null)
            {
                description = "Заявка снята со специалиста";
            }
            else if (ticket?.ResponsibleId == null)
            {
                description = $"Заявка назначена на сотрудника {customerSurname}";
            }
            else
            {
                description = $"Заявка переназначена на сотрудника {customerSurname}";
            }

            var ticketHistory = new TicketHistory(this.CurrentUser?.Id, description, ticketId, ticketStatus.Id,
                TicketHistoryType.ResponsibleTicket);
            await this._ticketHistoryRepository.InsertAsync(ticketHistory);
            ticket.AssignResponsible(employee?.Id);
            await this._ticketsRepository.UpdateAsync(ticket, true);
            return this.ObjectMapper.Map<EmployeeProfile, EmployeeProfileDto>(employee);
        }
    }

    [Authorize(SpecialistPermissions.Profiles.Delete)]
    public async Task RemoveAssignedSpecialistAsync(long ticketId, Guid? employeeProfileId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
            var customer =
                await this._employeeProfileRepository.FirstOrDefaultAsync(x => x.Id == employeeProfileId);
            if (ticket.ResponsibleId == employeeProfileId)
            {
                var ticketHistory = new TicketHistory(customer?.IdentityUserId, "Снят специалист по заявки", ticketId,
                    ticket.TicketStatusId, TicketHistoryType.ResponsibleTicket);
                await this._ticketHistoryRepository.InsertAsync(ticketHistory);
                ticket.AssignResponsible(null);
                await this._ticketsRepository.UpdateAsync(ticket, true);
            }
        }
    }

    [Authorize(SpecialistPermissions.Profiles.Get)]
    public async Task<EmployeeProfileDto> GetSpecialistByTicketIdAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = (await this._ticketsRepository
                    .WithDetailsAsync(x => x.Responsible))
                .FirstOrDefault(x => x.Id == ticketId);
            return this.ObjectMapper.Map<EmployeeProfile, EmployeeProfileDto>(ticket.Responsible);
        }
    }

    [Authorize(TicketPermissions.Tickets.Get)]
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
                x => x.TicketSection,
                x => x.TicketType)).FirstOrDefault(x => x.Id == ticketId);
            if (ticket == null)
            {
                return null;
            }

            var ticketDetailsDto = this.ObjectMapper.Map<Ticket, TicketDetailsDto>(ticket);
            var employeeProfile = await this._employeeProfileRepository
                .FirstOrDefaultAsync(x => x.IdentityUserId == ticket.CreatorId);

            if (employeeProfile is not null)
            {
                ticketDetailsDto.CreatorJobPost = employeeProfile?.JobPost;
                ticketDetailsDto.CreatorLastName = employeeProfile?.LastName;
                ticketDetailsDto.CreatorFirstName = employeeProfile?.FirstName;
                ticketDetailsDto.CreatorMiddleName = employeeProfile?.MiddleName;
            }
            else
            {
                var user = await _customerUserProfileRepository.FirstOrDefaultAsync(x => x.Id == ticket.CreatorId);

                ticketDetailsDto.CreatorJobPost = user?.JobPost;
                ticketDetailsDto.CreatorLastName = user?.LastName;
                ticketDetailsDto.CreatorFirstName = user?.FirstName;
                ticketDetailsDto.CreatorMiddleName = user?.MiddleName;
            }

            ticketDetailsDto.ServicePackages = ticket.ServicePackages;

            var ticketTags = (await this._ticketTagRepository
                    .WithDetailsAsync(x => x.Tag))
                .Where(x => x.TicketId == ticket.Id)
                .Select(x => x.Tag)
                .ToList();

            ticketDetailsDto.Tags = this.ObjectMapper.Map<List<Tag>, List<TagDto>>(ticketTags);
            return ticketDetailsDto;
        }
    }

    [Authorize(TicketPermissions.Tickets.Create)]
    public async Task<TicketDetailsDto> CreateTicketAsync(CreateUpdateTicketDto dto)
    {
        using (this._dataFilter.Enable<IMultiTenant>())
        {
            var ticket = new Ticket(dto.Subject, dto.Description, dto.TicketTypeId, dto.TicketStatusId, null)
            {
                CreatorId = dto.CreatorId
            };
            if (dto.TenantId != null)
            {
                ticket.SetTenant(dto.TenantId);
            }
            if (dto.ContractId != Guid.Empty)
            {
                ticket.ContractId = dto.ContractId;
            }
            else
            {
                throw new Volo.Abp.UserFriendlyException("Нет действующего договора");
            }

            await this._ticketsRepository.InsertAsync(ticket);
            await this.CurrentUnitOfWork.SaveChangesAsync();
            var uploadedFiles = this.ObjectMapper.Map<List<UploadedFileDto>, List<UploadedFile>>(dto.Attachments);
            return this.ObjectMapper.Map<Ticket, TicketDetailsDto>(ticket);
        }
    }

    [Authorize(TicketPermissions.Tickets.Get)]
    public async Task MakeReadTicketByIdAsync(long ticketId, UpdateReadTicketDto dto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
            ticket.ReadByClientDate = dto.ReadByClientDate;
            ticket.ReadBySpecialistDate = dto.ReadBySpecialistDate;
            await this._ticketsRepository.UpdateAsync(ticket, true);
        }
    }

    [Authorize(TicketPermissions.Tickets.Get)]
    public async Task<List<TicketResponsibleCountDto>> GetResponsibleWithTicketCountByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var status = (await _ticketStatusRepository.WithDetailsAsync()).Where(x => x.Name == TicketStatusConstants.InProgress).FirstOrDefault();
            var ticketData = (await this._ticketsRepository.WithDetailsAsync(x => x.Responsible))
                .Where(x => x.CreationTime > startDate && x.CreationTime < endDate && x.ResponsibleId != null && x.TicketStatusId == status.Id);

            var groupedData = new List<TicketResponsibleCountDto>();
            foreach (var ticket in ticketData)
            {
                var ticketCount = (await _ticketsRepository.GetQueryableAsync()).Count(x => x.ResponsibleId == ticket.ResponsibleId && x.TicketStatusId == status.Id);
                if (groupedData.All(x => x.Id != ticket.ResponsibleId))
                {
                    groupedData.Add(new TicketResponsibleCountDto
                    {
                        FirstName = ticket.Responsible.FirstName,
                        LastName = ticket.Responsible.LastName,
                        MiddleName = ticket.Responsible.MiddleName,
                        Id = ticket.Responsible.Id,
                        CountTicketTake = ticketCount
                    });
                }
            }

            return groupedData;
        }
    }

    private async Task CreateTicketTypeByMonthAsync(Guid? contractId, Guid newTicketTypeId, Guid nowTicketTypeId)
    {
        if (newTicketTypeId == nowTicketTypeId ||
            contractId == null)
        {
            return;
        }

        if (nowTicketTypeId != Guid.Empty)
        {
            var ticketTypeOldByMonth = await this._contractTicketsByTicketTypeByMonth.FirstOrDefaultAsync(x =>
                x.ContractId == contractId &&
                x.TicketTypeId == nowTicketTypeId &&
                x.Year == this.Clock.Now.Year &&
                x.Month == this.Clock.Now.Month);
            if (ticketTypeOldByMonth != null)
            {
                ticketTypeOldByMonth.Count -= 1;
                await this._contractTicketsByTicketTypeByMonth.UpdateAsync(ticketTypeOldByMonth, true);
            }
        }
    }

    private async Task CreateTicketSectionByMonthAsync(Guid? contractId, Guid? newTicketSectionId, Guid? nowTicketSectionId)
    {
        if (newTicketSectionId == nowTicketSectionId ||
            contractId == null)
        {
            return;
        }

        if (nowTicketSectionId != null)
        {
            var ticketSectionOldByMonth = await this._contractTicketsByTicketSectionByMonth.FirstOrDefaultAsync(x =>
                x.ContractId == contractId &&
                x.TicketSectionId == nowTicketSectionId &&
                x.Year == this.Clock.Now.Year &&
                x.Month == this.Clock.Now.Month);
            if (ticketSectionOldByMonth != null)
            {
                ticketSectionOldByMonth.Count -= 1;
                await this._contractTicketsByTicketSectionByMonth.UpdateAsync(ticketSectionOldByMonth, true);
            }
        }
    }

    private DateTime CalculateDeadline(DateTime startDate, TimeSpan duration, List<DateTime> holidays)
    {
        DateTime deadline = startDate + duration;

        int daysToAdd = 0;
        while (daysToAdd < duration.Days)
        {
            DateTime tempDate = startDate.AddDays(daysToAdd);
            if (holidays.Contains(tempDate.Date) || tempDate.DayOfWeek == DayOfWeek.Saturday || tempDate.DayOfWeek == DayOfWeek.Sunday)
            {
                deadline = deadline.AddDays(1);
            }
            daysToAdd++;
        }

        return deadline;
    }
}
