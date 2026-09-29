namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.Extensions;
using KV.Server.Interfaces;
using KV.Server.Localization;
using KV.Server.Permissions;
using KV.Server.Permissions.Calendars;
using KV.Server.Permissions.Contracts;
using KV.Server.Permissions.Events;
using KV.Server.Permissions.Stories;
using KV.Server.Permissions.Tickets;
using KV.Server.Profiles;
using KV.Server.Repositories;
using KV.Server.TicketHoursSpent;
using KV.Server.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

/// <summary>
///     Реализует встроеннные методы ICRUDCustomerAppService для создания, обновления, удаления записей.
/// </summary>
public class CrudCustomerAppService :
    CrudAppService<
        TenantProfile,
        CustomerDto,
        Guid,
        GetCustomerListRequestDto,
        CreateUpdateCustomerDto>,
    ICrudCustomerAppService
{
    private readonly IClock _clock;
    private readonly IRepository<ContractConstraintsByMonth, long> _contractConstraintsByMonth;
    private readonly IRepository<ConstraintType, Guid> _constraintTypeRepository;
    private readonly IStringLocalizer<ServerResource> _localizer;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IRepository<ContractStatus, Guid> _contractStatusRepository;
    private readonly IRepository<ContractServicePackage> _contractServicePackage;

    private readonly IRepository<ContractTicketsByTicketSectionByMonth, long>
        _contractTicketsByTicketSectionByMonthRepository;

    private readonly IRepository<ContractTicketsByTicketTypeByMonth, long>
        _contractTicketsByTicketTypeByMonthRepository;

    private readonly ITicketHistoryRepository _ticketHistoryRepository;
    private readonly IRepository<Ticket, long> _ticketRepository;
    private readonly IRepository<TicketSection, Guid> _ticketSectionRepository;
    private readonly IRepository<TicketType, Guid> _ticketTypeRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRepository<CustomerUserProfile, Guid> _customerUserProfileRepository;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IPermissionManager _permissionManager;

    public CrudCustomerAppService(IRepository<TenantProfile, Guid> repository,
        IRepository<Contract, Guid> contractRepository,
        IRepository<Ticket, long> ticketRepository,
        IRepository<TicketType, Guid> ticketTypeRepository,
        IRepository<TicketSection, Guid> ticketSectionRepository,
        IRepository<ConstraintType, Guid> constraintTypeRepository,
        ITicketHistoryRepository ticketHistoryRepository,
        IRepository<ContractConstraintsByMonth, long> contractConstraintsByMonth,
        IRepository<ContractTicketsByTicketSectionByMonth, long> contractTicketsByTicketSectionByMonthRepository,
        IRepository<ContractTicketsByTicketTypeByMonth, long> contractTicketsByTicketTypeByMonthRepository,
        IRepository<CustomerUserProfile, Guid> customerUserProfileRepository,
        IClock clock,
        IUnitOfWorkManager unitOfWorkManager,
        IRepository<ContractServicePackage> contractServicePackage,
        IdentityRoleManager identityRoleManager,
        IPermissionManager permissionManager,
        IRepository<ContractStatus, Guid> contractStatusRepository,
        IStringLocalizer<ServerResource> localizer) : base(repository)
    {
        this._contractRepository = contractRepository;
        this._ticketRepository = ticketRepository;
        this._ticketTypeRepository = ticketTypeRepository;
        this._ticketSectionRepository = ticketSectionRepository;
        this._constraintTypeRepository = constraintTypeRepository;
        this._ticketHistoryRepository = ticketHistoryRepository;
        this._contractConstraintsByMonth = contractConstraintsByMonth;
        this._contractTicketsByTicketSectionByMonthRepository = contractTicketsByTicketSectionByMonthRepository;
        this._contractTicketsByTicketTypeByMonthRepository = contractTicketsByTicketTypeByMonthRepository;
        this._customerUserProfileRepository = customerUserProfileRepository;
        this._clock = clock;
        this._unitOfWorkManager = unitOfWorkManager;
        this._contractServicePackage = contractServicePackage;
        _identityRoleManager = identityRoleManager;
        _permissionManager = permissionManager;
        _contractStatusRepository = contractStatusRepository;
        _localizer = localizer;
    }

    public const string IsActiveLowerCaseName = "active";

    [Authorize(CustomerPermissions.Profiles.Delete)]
    public override async Task DeleteAsync(Guid id)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var tickets = (await _ticketRepository.GetQueryableAsync())
            .Where(x => x.TenantId == id);
            await _ticketRepository.DeleteManyAsync(tickets);

            var contracts = (await _contractRepository.GetQueryableAsync())
                .Where(x => x.TenantId == id);
            await _contractRepository.DeleteManyAsync(contracts);

            var users = (await _customerUserProfileRepository.GetQueryableAsync())
                .Where(x => x.TenantId == id);
            await _customerUserProfileRepository.DeleteManyAsync(users);

            await base.DeleteAsync(id);
        }
    }

    [Authorize(CustomerPermissions.Profiles.Get)]
    public override async Task<CustomerDto> GetAsync(Guid id)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var tenantProfile = (await this.Repository.WithDetailsAsync(
                x => x.Region,
                x => x.TenantProfileManagers,
                x => x.ContactUserProfile,
                x => x.ResponsibleManager))
                .FirstOrDefault(x => x.Id == id) ?? throw new BusinessException("Клиент не найден");

            var dto = this.ObjectMapper.Map<TenantProfile, CustomerDto>(tenantProfile);
            dto.ResponsibleFullName = $"{tenantProfile.ResponsibleManager?.LastName} " +
                $"{tenantProfile.ResponsibleManager?.FirstName} " +
                $"{tenantProfile.ResponsibleManager?.MiddleName}";
            dto.ResponsibleManager =
                $"{tenantProfile.ResponsibleManager?.LastName} " +
                $"{tenantProfile.ResponsibleManager?.FirstName} " +
                $"{tenantProfile.ResponsibleManager?.MiddleName}";
            return dto;
        }
    }

    [Authorize(CustomerPermissions.Profiles.Get)]
    public override async Task<PagedResultDto<CustomerDto>> GetListAsync(
        GetCustomerListRequestDto input)
    {
        var result = await this.TryDisableMultiTenantAsync(async () =>
        {
            var searchIsActive = input.SearchByActive == IsActiveLowerCaseName;
            var query = await this.Repository.WithDetailsAsync(
                x => x.Region,
                x => x.TenantProfileManagers,
                x => x.ResponsibleManager);
            query = query
                .Where(x => string.IsNullOrWhiteSpace(input.ManagerId)
                            || x.ResponsibleManagerId == Guid.Parse(input.ManagerId)
                            || x.TenantProfileManagers.Any(x => x.ManagerId == Guid.Parse(input.ManagerId)))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchByActive)
                            || x.IsActive == searchIsActive)
                .Where(x => string.IsNullOrWhiteSpace(input.SearchLongName) ||
                            x.LongName.ToLower().Contains(input.SearchLongName.Trim().ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchShortName) ||
                            x.ShortName.ToLower().Contains(input.SearchShortName.Trim().ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchAddress) ||
                            x.Address.ToLower().Contains(input.SearchAddress.Trim().ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchStr)
                            || x.LongName.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.Address.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.ShortName.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.INNNumber.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.KPPNumber.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.Region.Name.ToLower().Contains(input.SearchStr.Trim().ToLower()));
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }
            else
            {
                query = query.OrderByDescending(x => x.ShortName);
            }

            var totalCount = query.Count();
            var entities = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount == 0 ? GetContractListRequestDto.DefaultPageSize : input.MaxResultCount)
                .ToList();
            var dtos = this.ObjectMapper.Map<List<TenantProfile>, List<CustomerDto>>(entities);
            return new PagedResultDto<CustomerDto>(totalCount, dtos);
        });
        return result;
    }
    [Authorize(CustomerPermissions.Profiles.Get)]
    public async Task<List<CustomerStatisticsDto>> GetStatisticsAsync(GetCustomerStatisticsListRequestDto input)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var ticketSections = await this._ticketSectionRepository.ToListAsync();
            var ticketTypes = await this._ticketTypeRepository.ToListAsync();
            var constraintTypes = await this._constraintTypeRepository.ToListAsync();

            var ticketSectionContracts =
                (await this._contractTicketsByTicketSectionByMonthRepository.WithDetailsAsync(x => x.Contract))
                    .Where(x => x.Contract.TenantId == input.TenantId)
                    .GroupBy(c => new {
                        c.Year,
                        c.Month,
                        c.TicketSectionId
                    })
                    .Select(x => new { x.Key.Year, x.Key.Month, x.Key.TicketSectionId, Sum = x.Sum(p => p.Count) })
                    .ToList();

            var ticketTypeContracts =
                (await this._contractTicketsByTicketTypeByMonthRepository.WithDetailsAsync(x => x.Contract))
                    .Where(x => x.Contract.TenantId == input.TenantId)
                    .GroupBy(c => new {
                        c.Year,
                        c.Month,
                        c.TicketTypeId
                    })
                    .Select(x => new { x.Key.Year, x.Key.Month, x.Key.TicketTypeId, Sum = x.Sum(p => p.Count) })
                    .ToList();

            var constraintTypeContracts =
                (await this._contractConstraintsByMonth.WithDetailsAsync(x => x.Contract))
                    .Where(x => x.Contract.TenantId == input.TenantId)
                    .GroupBy(c => new {
                        c.Year,
                        c.Month,
                        c.ConstraintTypeId,
                        c.MaxCount
                    })
                    .Select(x => new { x.Key.Year, x.Key.Month, x.Key.ConstraintTypeId, x.Key.MaxCount, Sum = x.Sum(p => p.Sum) })
                    .ToList();
            input.StartPeriod = new DateTime(2023, 01, 01);
            input.EndPeriod = DateTime.Now;
            var range = input.StartPeriod.MonthRangeTo(input.EndPeriod).Select(p => new { p.Year, p.Month }).ToList();
            var joined = range.GroupJoin(ticketSectionContracts, x => new { x.Year, x.Month }, y => new { y.Year, y.Month },
                (x, y) =>
                {
                    return new CustomerStatisticsDto()
                    {
                        Month = x.Month,
                        Year = x.Year,
                        TicketSectionsByMonths = y.Select(s => new ContractTicketsByTicketSectionByMonthDto()
                        {
                            Count = s.Sum,
                            Name = ticketSections.FirstOrDefault(ss => ss.Id == s.TicketSectionId).Name
                        }).ToList(),
                    };
                }
            ).GroupJoin(ticketTypeContracts, x => new { x.Year, x.Month }, y => new { y.Year, y.Month }, (x, y) =>
            {
                return new CustomerStatisticsDto()
                {
                    Month = x.Month,
                    Year = x.Year,
                    TicketSectionsByMonths = x.TicketSectionsByMonths,
                    TicketTypesByMonths = y.Select(s => new ContractTicketsByTicketTypeByMonthDto()
                    {
                        Count = s.Sum,
                        Name = ticketTypes.FirstOrDefault(ss => ss.Id == s.TicketTypeId).Name
                    }
                    ).ToList()
                };
            }).GroupJoin(constraintTypeContracts, x => new { x.Year, x.Month }, y => new { y.Year, y.Month }, (x, y) =>
            {
                return new CustomerStatisticsDto()
                {
                    Month = x.Month,
                    Year = x.Year,
                    TicketSectionsByMonths = x.TicketSectionsByMonths,
                    TicketTypesByMonths = x.TicketTypesByMonths,
                    ConstraintsByMonths = y.Select(s => new ContractConstraintsByMonthDto()
                    {
                        MaxCount = s.MaxCount,
                        Sum = s.Sum,
                        Name = constraintTypes.FirstOrDefault(ss => ss.Id == s.ConstraintTypeId).Name
                    }
                    ).ToList().Union(constraintTypes.Select(x => x.Id).Except(y.Select(s => s.ConstraintTypeId)).Select(s => new ContractConstraintsByMonthDto()
                    {
                        MaxCount = 0,
                        Sum = 0,
                        Name = constraintTypes.FirstOrDefault(ss => ss.Id == s).Name
                    }).ToList(), new ContractConstraintsByMonthDtoComparer()).ToList()
                };
            }).ToList();

            return joined
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();
        }
    }

    [Authorize]
    public async Task<List<CustomerContractStatisticsDto>> GetStatisticsConstraintByContractIdAsync(Guid contractId)
    {
        var constraintTypeContracts =
            (await this._contractConstraintsByMonth.WithDetailsAsync())
            .Where(x => x.ContractId == contractId)
            .GroupBy(c => new {
                c.Year,
                c.Month,
                c.ConstraintTypeId,
                c.MaxCount
            })
            .Select(x => new { x.Key.Year, x.Key.Month, x.Key.ConstraintTypeId, x.Key.MaxCount, Sum = x.Sum(p => p.Sum) })
            .ToList();

        var constraintTypes = await this._constraintTypeRepository.ToListAsync();

        var resultStatistics = new List<CustomerContractStatisticsDto>();
        foreach (var constraintType in constraintTypeContracts)
        {
            var statisticIndex =
                resultStatistics.FindIndex(x => constraintType.Year == x.Year && constraintType.Month == x.Month);
            var name = constraintTypes.FirstOrDefault(x => x.Id == constraintType.ConstraintTypeId).Name;
            if (statisticIndex == -1)
            {
                resultStatistics.Add(new()
                {
                    Month = constraintType.Month,
                    Year = constraintType.Year
                });
                statisticIndex = resultStatistics.Count - 1;
            }

            resultStatistics[statisticIndex].ConstraintsByMonths.Add(new ContractConstraintsByMonthDto
            {
                MaxCount = constraintType.MaxCount,
                Sum = constraintType.Sum,
                Name = name
            });
        }

        return resultStatistics
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToList();
    }

    [Authorize(CustomerPermissions.Profiles.Get)]
    public async Task<PagedResultDto<CustomerEventDto>> GetCustomerEventsAsync(GetCustomerEventsListRequestDto input)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var ticketHistoryQuery = await this._ticketHistoryRepository.IncludeTicketWithCreatorAsync();
            ticketHistoryQuery = ticketHistoryQuery.Where(x => x.Ticket.TenantId == input.TenantId);
            ticketHistoryQuery =
                ticketHistoryQuery.Where(x => input.StartPeriod < x.CreationTime && input.EndPeriod > x.CreationTime);
            var totalCount = ticketHistoryQuery.Count();
            var actuallyTicketHistoryClient = ticketHistoryQuery
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount == 0
                    ? GetCustomerEventsListRequestDto.DefaultPageSize
                    : input.MaxResultCount)
                .ToList();
            var resultEventsDto = new List<CustomerEventDto>();
            foreach (var ticketHistory in actuallyTicketHistoryClient)
            {
                var eventDto = new CustomerEventDto();
                var ticketId = ticketHistory.TicketId;
                eventDto.CreationTime = ticketHistory.CreationTime;
                if (ticketHistory.Type == TicketHistoryType.ResponsibleTicket)
                {
                    eventDto.Action = $"Назначает ответственного на заявку № {ticketId}";
                    if (ticketHistory.Ticket.ResponsibleId == null)
                    {
                        eventDto.Action = $"Закрывает заявку № {ticketId}";
                    }
                }
                else if (ticketHistory.Type == TicketHistoryType.ChangeLimit)
                {
                    eventDto.Action = $"{ticketHistory.Description}";
                }
                else if (ticketHistory.Type == TicketHistoryType.HiddenForClient)
                {
                    eventDto.Action = $"Отправляет скрытое для клиента сообщение: {ticketHistory.Description} ";
                }
                else
                {
                    eventDto.Action = $"Оставляет комментарий к заявке № {ticketId}";
                }

                eventDto.Subject = ticketHistory.Ticket?.Subject == null ? "" : $"“{ticketHistory.Ticket.Subject}”";
                eventDto.CreatorFirstName = ticketHistory?.Creator?.Name ?? string.Empty;
                eventDto.CreatorLastName = ticketHistory?.Creator?.Surname ?? string.Empty;
                eventDto.Url = $"/tickets/details/{ticketId}";
                eventDto.Description = ticketHistory.Description;
                resultEventsDto.Add(eventDto);
            }

            return new PagedResultDto<CustomerEventDto>(totalCount, resultEventsDto);
        }
    }

    [Authorize(CustomerPermissions.Profiles.Edit)]
    public override Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input) => base.UpdateAsync(id, input);
    private async Task CreateContractStatusAsync(Guid tenantId, string title, string code) => await this._contractStatusRepository.InsertAsync(new ContractStatus
    {
        TenantId = tenantId,
        Title = title,
        Code = code
    }, true);


    [Authorize(CustomerPermissions.Profiles.Create)]
    [UnitOfWork(IsDisabled = true)]
    public override async Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
    {
        using (var uow = UnitOfWorkManager.Begin(true, true))
        {
            CustomerDto createdCustomer;
            using (this.DataFilter.Disable<IMultiTenant>())
            {
                createdCustomer = await base.CreateAsync(input);
            }
            await uow.SaveChangesAsync();

            using (CurrentTenant.Change(createdCustomer.Id))
            {
                await CreateClientRoleAsync(createdCustomer.Id);
            }
            await this.CreateContractStatusAsync(input.TenantId, this._localizer["ContractStatus:Code:Draft"], "draft");
            await this.CreateContractStatusAsync(input.TenantId, this._localizer["ContractStatus:Code:Cancel"], "cancel");
            await this.CreateContractStatusAsync(input.TenantId, this._localizer["ContractStatus:Code:Complete"], "complete");
            await this.CreateContractStatusAsync(input.TenantId, this._localizer["ContractStatus:Code:Progress"], "in progress");

            await uow.CompleteAsync();
            return createdCustomer;
        }
    }

    [Authorize(CustomerPermissions.Profiles.Get)]
    public async Task<PagedResultDto<ContractDto>> GetContractsByCustomerIdAsync(
        GetCustomerContractListRequestDto input) => await this.TryDisableMultiTenantAsync(async () =>
                                                         {
                                                             var query = (await this._contractRepository.WithDetailsAsync(x => x.TenantProfile, x => x.Status, x => x.ServicePackages))
                                                                .AsQueryable();
                                                             query = query.Where(x => x.TenantId == input.TenantId);
                                                             query = query
                                                                 .Where(x => string.IsNullOrWhiteSpace(input.SearchName) || x.Name.Contains(input.SearchName.Trim()))
                                                                 .Where(x => string.IsNullOrWhiteSpace(input.SearchContractStartDate) ||
                                                                             x.ContractStartDate > DateTime.Parse(input.SearchContractStartDate))
                                                                 .Where(x => string.IsNullOrWhiteSpace(input.SearchContractFinishDate) ||
                                                                             x.ContractFinishDate < DateTime.Parse(input.SearchContractFinishDate))
                                                                 .Where(x => string.IsNullOrWhiteSpace(input.SearchStatusId) ||
                                                                             x.Status.Id.ToString() == input.SearchStatusId);
                                                             if (!string.IsNullOrEmpty(input.Sorting))
                                                             {
                                                                 if (input.Sorting == "statusTitle asc")
                                                                 {
                                                                     query = query.OrderBy(x => x.Status.Title);
                                                                 }
                                                                 else if (input.Sorting == "statusTitle desc")
                                                                 {
                                                                     query = query.OrderByDescending(x => x.Status.Title);
                                                                 }
                                                                 else
                                                                 {
                                                                     query = query.OrderBy(input.Sorting);
                                                                 }
                                                             }
                                                             else
                                                             {
                                                                 query = query.OrderByDescending(x => x.CreationTime);
                                                             }

                                                             var totalCount = query.Count();
                                                             var entities = query
                                                                 .Skip(input.SkipCount)
                                                                 .Take(input.MaxResultCount == 0 ? GetContractListRequestDto.DefaultPageSize : input.MaxResultCount)
                                                                 .ToList();
                                                             var dtos = this.ObjectMapper.Map<List<Contract>, List<ContractDto>>(entities);
                                                             return new PagedResultDto<ContractDto>(totalCount, dtos);
                                                         });

    [Authorize(CustomerPermissions.Profiles.Edit)]
    public async Task UpdateResponsibleManagerByTenantIdAsync(Guid? customerUserProfileId, Guid tenantId)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var tenantProfile = await this.Repository.FirstOrDefaultAsync(x => x.Id == tenantId);
            if (tenantProfile != null)
            {
                tenantProfile.ResponsibleManagerId = customerUserProfileId;
                await this.Repository.UpdateAsync(tenantProfile);
            }
        }
    }

    [Authorize(CustomerPermissions.Profiles.Edit)]
    public async Task UpdateContactUserProfileAsync(Guid? customerUserProfileId, Guid tenantId)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var tenantProfile = await this.Repository.FirstOrDefaultAsync(x => x.Id == tenantId);
            if (tenantProfile != null)
            {
                var user = await _customerUserProfileRepository.FirstOrDefaultAsync(x => x.Id == customerUserProfileId);
                tenantProfile.ContactUserProfile = user;
                await this.Repository.UpdateAsync(tenantProfile);
            }
        }
    }

    public async Task<CustomerDto> FirstOrDefaultByTenantIdAsync(Guid? tenantId)
    {
        var customer = await this.Repository.FirstOrDefaultAsync(x => x.Id == tenantId);
        var dto = this.ObjectMapper.Map<TenantProfile, CustomerDto>(customer);
        return dto;
    }

    [Authorize(CustomerPermissions.Profiles.Get)]
    public async Task<List<CustomerServicePackageDto>> GetActiveCustomerServicePackagesAsync(Guid tenantId)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var contracts = (await this._contractRepository
                    .WithDetailsAsync(x => x.ServicePackages))
                .Where(x => x.TenantId == tenantId)
                .Where(x => x.ContractFinishDate > this._clock.Now)
                .ToList();
            var servicePacakges = new List<CustomerServicePackageDto>();

            foreach (var contract in contracts)
            {

                foreach (var item in contract.ServicePackages)
                {
                    var package = (await this._contractServicePackage.WithDetailsAsync(x => x.ServicePackage)).FirstOrDefault(x => x.ServicePackageId == item.ServicePackageId);
                    servicePacakges.Add(new CustomerServicePackageDto
                    {
                        Id = package.ServicePackage?.Id ?? Guid.Empty,
                        FinishDate = contract.ContractFinishDate,
                        ContractName = contract.Name,
                        Name = package.ServicePackage.Name,
                        Style = package.ServicePackage?.Style
                    });
                }

            }

            return servicePacakges;
        }
    }

    public async Task<int> GetCountResponsibleManagerByCustomerUserProfileIdAsync(Guid customerUserProfileId)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var query = await this.Repository.GetQueryableAsync();
            query = query.Where(x => x.ResponsibleManagerId == customerUserProfileId);
            var count = query.Count();
            return count;
        }
    }

    private async Task<T> TryDisableMultiTenantAsync<T>(Func<Task<T>> func)
    {
        if (this.CurrentTenant.Id is null)
        {
            using (this.DataFilter.Disable<IMultiTenant>())
            {
                return await func();
            }
        }

        return await func();
    }
    [Authorize(CustomerPermissions.Profiles.Get)]
    public async Task<List<CustomerServicePackageDto>> GetActiveServicePackagesByContractIdAsync(Guid contractId)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var contractQuery = (await this._contractRepository
                    .WithDetailsAsync(x => x.ServicePackages));
            var contract = contractQuery.Where(x => x.ContractFinishDate > this._clock.Now)
                .FirstOrDefault(x => x.Id == contractId);
            var servicePacakges = new List<CustomerServicePackageDto>();
            if (contract != null)
            {
                foreach (var servicePackage in contract.ServicePackages)
                {
                    var package = (await this._contractServicePackage.WithDetailsAsync(x => x.ServicePackage)).FirstOrDefault(x => x.ServicePackageId == servicePackage.ServicePackageId);
                    servicePacakges.Add(new CustomerServicePackageDto
                    {
                        Id = package.ServicePackage?.Id ?? Guid.Empty,
                        FinishDate = contract.ContractFinishDate,
                        ContractName = contract.Name,
                        Name = package.ServicePackage.Name,
                        Style = package.ServicePackage?.Style
                    });

                }
            }


            return servicePacakges;
        }
    }

    public async Task<PagedResultDto<CustomerDto>> GetListCustomersBySearchAsync(GetCustomerListRequestDto input)
    {
        var result = await this.TryDisableMultiTenantAsync(async () =>
        {
            var searchIsActive = input.SearchByActive == IsActiveLowerCaseName;
            var query = await this.Repository.WithDetailsAsync(
                x => x.Region,
                x => x.TenantProfileManagers,
                x => x.ResponsibleManager);
            if (!string.IsNullOrEmpty(input.SearchResponsibleManagerId))
            {
                query = query.Where(x => x.ResponsibleManager.IdentityUserId == input.SearchResponsibleManagerId.To<Guid>());
            }
            query = query
                .Where(x => string.IsNullOrWhiteSpace(input.ManagerId)
                            || x.ResponsibleManagerId == Guid.Parse(input.ManagerId)
                            || x.TenantProfileManagers.Any(x => x.ManagerId == Guid.Parse(input.ManagerId)))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchByActive)
                            || x.IsActive == searchIsActive)
                .Where(x => string.IsNullOrWhiteSpace(input.SearchLongName) ||
                            x.LongName.ToLower().Contains(input.SearchLongName.Trim().ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchShortName) ||
                            x.ShortName.ToLower().Contains(input.SearchShortName.Trim().ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchAddress) ||
                            x.Address.ToLower().Contains(input.SearchAddress.Trim().ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchStr)
                            || x.LongName.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.ResponsibleManager.LastName.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.Address.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.ShortName.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.ContactUserProfile.Email.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.INNNumber.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.KPPNumber.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.ContactUserProfile.FirstName.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.ContactUserProfile.LastName.ToLower().Contains(input.SearchStr.Trim().ToLower())
                            || x.Region.Name.ToLower().Contains(input.SearchStr.Trim().ToLower()));

            if (!string.IsNullOrEmpty(input.SearchResponsibleManager))
            {
                query = query.Where(x => x.ResponsibleManager.LastName == input.SearchResponsibleManager);
            }
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }
            else
            {
                query = query.OrderByDescending(x => x.ShortName);
            }

            var totalCount = query.Count();
            var entities = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount == 0 ? GetContractListRequestDto.DefaultPageSize : input.MaxResultCount)
                .ToList();
            var dtos = this.ObjectMapper.Map<List<TenantProfile>, List<CustomerDto>>(entities);
            return new PagedResultDto<CustomerDto>(totalCount, dtos);
        });
        return result;
    }

    public async Task CreateClientRoleAsync(Guid customerId)
    {
        Guid newRoleId = GuidGenerator.Create();
        var newIdentityRole = new IdentityRole(newRoleId, UserRoleConstants.CLIENT_ROLE, customerId);
        var createdIdentityRole = await _identityRoleManager.CreateAsync(newIdentityRole);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, ContractPermissions.Contracts.Get, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, TicketPermissions.Tickets.Edit, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, TicketPermissions.Tickets.Create, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, StoriesPermissions.Stories.Get, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, EventPermissions.Events.Get, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, CalendarPermissions.Calendars.Get, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, CalendarPermissions.Calendars.Create, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, CalendarPermissions.Calendars.Edit, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, CalendarPermissions.Calendars.Delete, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, TicketAttachmentsPermissions.TicketAttachments.Create, true);
        await _permissionManager.SetForRoleAsync(UserRoleConstants.CLIENT_ROLE, TicketAttachmentsPermissions.TicketAttachments.Get, true);
    }
}
