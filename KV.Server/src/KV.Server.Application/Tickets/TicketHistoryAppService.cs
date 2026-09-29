namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.Dtos.File;
using KV.Server.Dtos.Tickets;
using KV.Server.File;
using KV.Server.Permissions;
using KV.Server.Profiles;
using KV.Server.Templates;
using KV.Server.TicketHoursSpent;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

public class TicketHistoryAppService : ApplicationService, ITicketHistoryAppService
{
    private readonly IRepository<AnswerTemplate, Guid> _answerTemplateRepository;
    private readonly IClock _clock;
    private readonly IRepository<ContractSetting> _contractSettingRepository;
    private readonly IRepository<CustomerUserProfile, Guid> _customerUserProfileRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<TicketHistory, Guid> _ticketHistoryRepository;
    private readonly IRepository<TicketStatus, Guid> _ticketStatusRepository;
    private readonly IRepository<TicketHoursSpentHistory, Guid> _ticketHoursSpentHistoryRepository;
    private readonly IRepository<Ticket, long> _ticketsRepository;
    private readonly IUsersAppService _usersAppService;
    private readonly IRepository<EmployeeProfile, Guid> _employeeProfileRepository;


    public TicketHistoryAppService(IRepository<Ticket, long> ticketsRepository,
        IRepository<TicketHistory, Guid> ticketHistoryRepository,
        IRepository<TicketHoursSpentHistory, Guid> ticketHoursSpentHistoryRepository,
        IRepository<AnswerTemplate, Guid> answerTemplateRepository,
        IRepository<CustomerUserProfile, Guid> customerUserProfileRepository,
        IRepository<ContractSetting> contractSettingRepository,
        IDataFilter dataFilter,
        IClock clock,
        IRepository<TicketStatus, Guid> ticketStatusRepository,
        IUsersAppService usersAppService,
        IRepository<EmployeeProfile, Guid> employeeProfileRepository)
    {
        this._ticketsRepository = ticketsRepository;
        this._ticketHistoryRepository = ticketHistoryRepository;
        this._ticketHoursSpentHistoryRepository = ticketHoursSpentHistoryRepository;
        this._answerTemplateRepository = answerTemplateRepository;
        this._customerUserProfileRepository = customerUserProfileRepository;
        this._contractSettingRepository = contractSettingRepository;
        this._dataFilter = dataFilter;
        this._clock = clock;
        _ticketStatusRepository = ticketStatusRepository;
        _usersAppService = usersAppService;
        _employeeProfileRepository = employeeProfileRepository;
    }

    public async Task<PagedResultDto<TicketHistoryDto>> GetTicketHistoryListAsync(GetTicketHistoryListRequestDto input,
        long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._ticketHistoryRepository.WithDetailsAsync(x => x.Creator);
            query = query.Where(x => x.TicketId == ticketId);
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }

            var totalCount = query.Count();
            var ticketsHistory = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount == 0 ? GetTicketHistoryListRequestDto.DefaultPageSize : input.MaxResultCount)
                .ToList();
            var ticketHistoryDtos = this.ObjectMapper.Map<List<TicketHistory>, List<TicketHistoryDto>>(ticketsHistory);
            return new PagedResultDto<TicketHistoryDto>(totalCount, ticketHistoryDtos);
        }
    }

    [Authorize(TicketHistoryPermissions.TicketHistory.Get)]
    public async Task<TicketHistoryDto> GetTicketHistoryAsync(Guid ticketHistoryId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticketHistory = await this._ticketHistoryRepository.FirstOrDefaultAsync(x => x.Id == ticketHistoryId);
            var dto = this.ObjectMapper.Map<TicketHistory, TicketHistoryDto>(ticketHistory);
            return dto;
        }
    }

    [Authorize(TicketHistoryPermissions.TicketHistory.Edit)]
    public async Task UpdateTicketHistoryDescriptionAsync(UpdateTicketHistoryDescriptionDto updateDto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticketHistory = await this._ticketHistoryRepository.FirstOrDefaultAsync(x => x.Id == updateDto.Id);
            ticketHistory.WriteDescription(updateDto.Description);
            await this._ticketHistoryRepository.UpdateAsync(ticketHistory);
        }
    }

    // TODO: enable tenant
    [Authorize(TicketHistoryPermissions.TicketHistory.Create)]
    public async Task<TicketHistoryDto> CreateTicketHistoryAsync(CreateTicketHistoryDto dto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == dto.TicketId);
            var statuses = (await _ticketStatusRepository.WithDetailsAsync());
            var status = statuses.FirstOrDefault(x => x.Id == ticket.TicketStatusId);
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

            ticket.UpdateStatus(dto.TicketStatusId, $"Статус заявки изменён на \"{status.DisplayName}\"", TicketHistoryType.StatusUpdate);
            await this._ticketsRepository.UpdateAsync(ticket, true);
            return this.ObjectMapper.Map<TicketHistory, TicketHistoryDto>(ticketHistory);
        }
    }
    [Authorize(TicketHistoryPermissions.TicketHistory.Create)]
    public async Task<TicketHistoryDto> CreateNotPickUpTicketHistoryAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == ticketId);
            var specialist = (await _employeeProfileRepository.GetQueryableAsync()).FirstOrDefault(x => x.Id == ticket.ResponsibleId);
            if(specialist == null)
            {
                throw new UserFriendlyException("Нет исполнителя заявки. Укажите исполнителя сначала");
            }
            var type = TicketHistoryType.NotPickUp;
            var ticketHistory = new TicketHistory(ticket.CreatorId,
               specialist != null ?
               $"Специалист не смог вам дозвониться с номера {specialist.Phone} в {this.Clock.Now:HH:mm}" : $"Специалист не смог вам дозвониться в {this.Clock.Now:HH:mm}",
               ticket.Id,
               ticket.TicketStatusId,
               type);
            await this._ticketHistoryRepository.InsertAsync(ticketHistory);
            await this.CurrentUnitOfWork.SaveChangesAsync();
            ticket.UpdateStatus(ticket.TicketStatusId, $"Специалист не смог дозвониться в {this.Clock.Now:HH:mm}", TicketHistoryType.NotPickUp);
            await this._ticketsRepository.UpdateAsync(ticket);

            return this.ObjectMapper.Map<TicketHistory, TicketHistoryDto>(ticketHistory);
        }
    }

    [Authorize(TicketHistoryPermissions.TicketHistory.Create)]
    public async Task<TicketHistoryDto> CreateAdminTicketHistoryAsync(CreateAdminTicketHistoryDto dto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticket = await this._ticketsRepository.FirstOrDefaultAsync(x => x.Id == dto.TicketId);
            var responsibleId = ticket?.ResponsibleId ?? Guid.Empty;
            var responsible = await this._customerUserProfileRepository.FirstOrDefaultAsync(x => x.Id == responsibleId);

            var answerTemplate = await this._answerTemplateRepository.FirstOrDefaultAsync(x => x.Id == dto.AnswerTemplateId);
            var ticketHistoryMessage = $"{answerTemplate?.Description} {dto?.Description}";

            var ticketHistory = new TicketHistory(dto.CreatorId, ticketHistoryMessage, dto.TicketId, dto.TicketStatusId, TicketHistoryType.SpecialistMessage);

            if (dto.IsInternal)
            {
                ticketHistory.SetInternal(true);
                ticketHistory.SetType(TicketHistoryType.HiddenForClient);
            }

            var createdTicketHistory = await this._ticketHistoryRepository.InsertAsync(ticketHistory);
            await this.CurrentUnitOfWork.SaveChangesAsync();

            if (dto.TicketStatusId != Guid.Empty)
            {
                var statuses = (await _ticketStatusRepository.WithDetailsAsync());
                var status = statuses.FirstOrDefault(x => x.Id == dto.TicketStatusId);
                ticket.UpdateStatus(dto.TicketStatusId, $"Статус заявки изменён на \"{status.DisplayName}\"", TicketHistoryType.StatusUpdate);
                await this._ticketsRepository.UpdateAsync(ticket);
            }

            if (ticket?.ContractId != Guid.Empty && dto.ConstraintTypeId != Guid.Empty)
            {
                var ticketHoursSpentHistory = new TicketHoursSpentHistory(dto.TicketHoursSpentHistoryCount, dto.ConstraintTypeId, createdTicketHistory.Id);
                var createdTicketHoursSpentHistory = await this._ticketHoursSpentHistoryRepository.InsertAsync(ticketHoursSpentHistory);
                await this.CurrentUnitOfWork.SaveChangesAsync();

                var lastContractSetting = (await this._contractSettingRepository.GetQueryableAsync())
                    .OrderByDescending(x => x.CreationTime)
                    .FirstOrDefault();

                if (this._clock.Now.Month != (lastContractSetting?.CreationTime.Month ?? -1))
                {
                    var newLastContractSetting = await this._contractSettingRepository.InsertAsync(new ContractSetting(lastContractSetting?.Max ?? dto.TicketHoursSpentHistoryCount + 10, dto.ConstraintTypeId, ticket.ContractId ?? Guid.Empty));
                    lastContractSetting = newLastContractSetting;
                }
            }

            return this.ObjectMapper.Map<TicketHistory, TicketHistoryDto>(ticketHistory);
        }
    }

    public async Task MakeReadTicketHistoryByIdAsync(Guid ticketHistoryId, UpdateReadTicketHistoryDto dto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var ticketHistory = await this._ticketHistoryRepository.FirstOrDefaultAsync(x => x.Id == ticketHistoryId);
            ticketHistory.ReadByClientDate = dto.ReadByClientDate;
            ticketHistory.ReadBySpecialistDate = dto.ReadBySpecialistDate;
            await this._ticketHistoryRepository.UpdateAsync(ticketHistory);
        }
    }

    public async Task<long> GetCountTicketHistoryAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._ticketHistoryRepository.GetQueryableAsync();
            query = query.Where(x => x.TicketId == ticketId);
            return query.LongCount();
        }
    }
}
