namespace KV.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Dtos.Employees;
using KV.Server.Dtos.Tickets;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface ITicketsAppService : IApplicationService
{
    Task<PagedResultDto<TicketListItemDto>> GetTicketsListAsync(GetTicketsListRequestDto input);
    Task<TicketDetailsDto> GetTicketDetailsAsync(long ticketId);
    Task<TicketDetailsDto> CreateTicketAsync(CreateUpdateTicketDto dto);
    Task<EmployeeProfileDto> GetSpecialistByTicketIdAsync(long ticketId);
    Task<EmployeeProfileDto> AssignedSpecialistAsync(long ticketId, Guid? employeeProfileDtoId);
    Task<List<TicketResponsibleCountDto>> GetResponsibleWithTicketCountByPeriodAsync(DateTime startDate, DateTime endDate);
    Task RemoveAssignedSpecialistAsync(long ticketId, Guid? employeeProfileId);
    Task UpdateTicketAsync(long ticketId, CreateUpdateTicketDto dto);
    Task UpdateTicketTypeByTicketIdAsync(long ticketId, Guid ticketTypeId);
    Task UpdateTicketSectionByTicketIdAsync(long ticketId, Guid? ticketSectionId);
    Task UpdateTicketStatusByTicketIdAsync(long ticketId, Guid ticketStatusId);
    Task UpdateTicketConstraintByTicketIdAsync(long ticketId, Guid constraintTypeId, int value);
    Task DeleteTicketAsync(long ticketId);
    Task MakeReadTicketByIdAsync(long ticketId, UpdateReadTicketDto dto);
    Task DeleteTicketHistoryRecordByIdAsync(Guid ticketHistoryId);
    Task UpdateTicketDescriptionAsync(long ticketId, UpdateTicketDescriptionDto dto);

}
