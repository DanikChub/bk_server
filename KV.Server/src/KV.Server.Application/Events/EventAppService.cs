namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Events;
using KV.Server.Permissions.Events;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

public class EventAppService : ApplicationService, IEventAppService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<CalendarEvent, Guid> _eventRepository;

    public EventAppService(IRepository<CalendarEvent, Guid> eventRepository,
        IDataFilter dataFilter,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        this._eventRepository = eventRepository;
        this._dataFilter = dataFilter;
        this._currentTenant = currentTenant;
        this._currentUser = currentUser;
    }

    [Authorize(EventPermissions.Events.Get)]
    public async Task<List<EventDto>> GetListEventsByTicketIdAsync(long ticketId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._eventRepository.GetQueryableAsync();
            query = query.Where(x => x.TicketId == ticketId);

            var entites = query.ToList();
            var dtos = this.ObjectMapper
                .Map<List<CalendarEvent>, List<EventDto>>(entites);
            return dtos;
        }
    }

    public async Task CreateEventAsync(CreateEventInTicketDto createDto)
    {
        var entity = this.ObjectMapper.Map<CreateEventInTicketDto, CalendarEvent>(createDto);
        await this._eventRepository.InsertAsync(entity);
    }

    [Authorize]
    public async Task<List<EventDto>> GetListEventsByPeriodAsync(DateTime startPeriod, DateTime endPeriod)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._eventRepository.WithDetailsAsync(x=> x.Creator);
            query = query.Where(x => x.TenantId == this._currentTenant.Id);
            query = query.Where(x => startPeriod < x.EventDate && endPeriod > x.EventDate);

            var entites = query.ToList();
            var dtos = this.ObjectMapper.Map<List<CalendarEvent>, List<EventDto>>(entites);
            return dtos;
        }
    }

    [Authorize]
    public async Task<List<EventDto>> GetListEventsByPeriodCurrentUserAsync(DateTime startPeriod, DateTime endPeriod)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._eventRepository.GetQueryableAsync();
            query = query.Where(x => x.CreatorId == this._currentUser.Id);
            query = query.Where(x => startPeriod < x.EventDate && endPeriod > x.EventDate);

            var entites = query.ToList();
            var dtos = this.ObjectMapper.Map<List<CalendarEvent>, List<EventDto>>(entites);
            return dtos;
        }
    }
}
