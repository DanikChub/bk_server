namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.Permissions;
using KV.Server.TicketHoursSpent;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

public class TicketHoursSpentHistoryAppService : ApplicationService, ITicketHoursSpentHistoryAppService
{
    private readonly IRepository<ConstraintType, Guid> _constraintTypeRepository;
    private readonly IRepository<ContractSetting> _contractSettingRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<TicketHistory, Guid> _ticketHistoryRepository;
    private readonly IRepository<TicketHoursSpentHistory, Guid> _ticketHoursSpentHistoryRepository;
    private readonly IRepository<Ticket, long> _ticketRepository;

    public TicketHoursSpentHistoryAppService(IRepository<TicketHistory, Guid> ticketHistoryRepository,
        IRepository<TicketHoursSpentHistory, Guid> ticketHoursSpentHistoryRepository,
        IRepository<ConstraintType, Guid> constraintTypeRepository,
        IRepository<ContractSetting> contractSettingRepository,
        IRepository<Ticket, long> ticketRepository,
        IDataFilter dataFilter)
    {
        this._ticketHistoryRepository = ticketHistoryRepository;
        this._ticketHoursSpentHistoryRepository = ticketHoursSpentHistoryRepository;
        this._constraintTypeRepository = constraintTypeRepository;
        this._contractSettingRepository = contractSettingRepository;
        this._ticketRepository = ticketRepository;
        this._dataFilter = dataFilter;
    }

    [Authorize(TicketHoursSpentHistoryPermissions.TicketHoursSpentHistory.Create)]
    public async Task<TicketHoursSpentHistoryDto> SetSpentTimeAsync(CreateUpdateTicketHoursSpentHistoryDto createDto)
    {
        var create = this.ObjectMapper.Map<CreateUpdateTicketHoursSpentHistoryDto, TicketHoursSpentHistory>(createDto);
        var entity = await this._ticketHoursSpentHistoryRepository.InsertAsync(create);
        var dto = this.ObjectMapper.Map<TicketHoursSpentHistory, TicketHoursSpentHistoryDto>(entity);
        var rangeType = await this._constraintTypeRepository.FirstOrDefaultAsync(x => x.Id == create.ConstraintTypeId);
        dto.ConstraintTypeName = rangeType.Name;
        return dto;
    }

    public async Task<List<ConstraintTypeContractDto>> ToListDisplayConstraintTypesContractId(Guid? contractId)
    {
        var query = await this._ticketHistoryRepository.WithDetailsAsync(
            x => x.TicketHoursSpentHistory,
            x => x.TicketHoursSpentHistory.ConstraintType,
            x => x.Ticket);
        query = query.Where(x => x.Ticket.ContractId == contractId);
        var ticketHistories = query
            .OrderByDescending(x => x.CreationTime)
            .ThenBy(x => x.TicketHoursSpentHistory.ConstraintType.Name)
            .ToList();
        var contractSettings = (await this._contractSettingRepository
                .WithDetailsAsync(x => x.ConstraintType))
            .Where(x => x.ContractId == contractId)
            .ToList();
        var constraintResultDto = new List<ConstraintTypeContractDto>();
        var lastMonth = -1;
        foreach (var ticketHistory in ticketHistories)
        {
            if (ticketHistory?.TicketHoursSpentHistory == null)
            {
                continue;
            }

            var contractSettingSpent = ticketHistory?.TicketHoursSpentHistory?.Spent ?? 0;
            var contractSettingConstraintName = ticketHistory?.TicketHoursSpentHistory?.ConstraintType?.Name;
            var contractSetting =
                contractSettings.FirstOrDefault(x => x.ConstraintType.Name == contractSettingConstraintName);
            var ticketHistoryMonth = ticketHistory.CreationTime.Month;
            var contractSettingMonth = contractSetting?.CreationTime.Month ?? ticketHistoryMonth;
            if (ticketHistoryMonth == contractSettingMonth)
            {
                if (lastMonth != ticketHistoryMonth)
                {
                    constraintResultDto.Add(new ConstraintTypeContractDto
                    {
                        Month = ticketHistoryMonth,
                        ConstraintTypes = new List<ConstraintTypeDto>()
                    });
                }

                var constraintTypes = constraintResultDto[^1].ConstraintTypes;
                var constraintType = constraintTypes.FirstOrDefault(x => x.Name == contractSettingConstraintName);
                if (constraintType != null)
                {
                    contractSettingSpent += constraintType.NowCount;
                    constraintTypes.RemoveAt(constraintTypes.Count - 1);
                }

                constraintTypes.Add(new ConstraintTypeDto
                {
                    NowCount = contractSettingSpent,
                    MaxCount = contractSetting?.Max ?? 10,
                    Name = contractSettingConstraintName
                });
                constraintResultDto[^1].ConstraintTypes = constraintTypes;
                lastMonth = ticketHistoryMonth;
            }
        }

        return constraintResultDto;
    }

    public async Task<List<ConstraintTypeDto>> ToListConstraintTypesAsync()
    {
        var ticketRanges = await this._constraintTypeRepository.ToListAsync();
        var dtos = this.ObjectMapper.Map<List<ConstraintType>, List<ConstraintTypeDto>>(ticketRanges);
        return dtos;
    }

    public async Task<List<ConstraintTypeDto>> ToListTicketRangeByTicketIdAsync(long ticketId)
    {
        var query = await this._ticketHistoryRepository.WithDetailsAsync(
            x => x.TicketHoursSpentHistory,
            x => x.TicketHoursSpentHistory.ConstraintType);
        query = query.Where(x => x.TicketId == ticketId);
        var ticketHoursSpentHistories = query
            .Select(x => x.TicketHoursSpentHistory)
            .ToList();
        var dtos = new List<ConstraintTypeDto>();
        Ticket ticket;
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            ticket = await this._ticketRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
        }

        foreach (var ticketHoursSpentHistory in ticketHoursSpentHistories)
        {
            if (ticketHoursSpentHistory?.ConstraintType?.Name != null)
            {
                var ticketRange = dtos.FirstOrDefault(x => x.Name == ticketHoursSpentHistory?.ConstraintType?.Name);
                if (ticketRange == null)
                {
                    var contractSetting =
                        await this._contractSettingRepository.FirstOrDefaultAsync(x => x.ContractId == ticket.ContractId);
                    ticketRange = new ConstraintTypeDto
                    {
                        Name = ticketHoursSpentHistory.ConstraintType.Name,
                        NowCount = ticketHoursSpentHistory.Spent,
                        MaxCount =
                        contractSetting?.Max ??
                        10 // TODO: Should create standart template for not found value (but that not need, because in ticket not display this)
                    };
                    dtos.Add(ticketRange);
                }
                else
                {
                    ticketRange.NowCount += ticketHoursSpentHistory.Spent;
                }
            }
        }

        return dtos;
    }
}
