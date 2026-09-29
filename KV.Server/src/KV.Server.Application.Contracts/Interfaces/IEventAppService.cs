namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IEventAppService
{
    Task<List<EventDto>> GetListEventsByTicketIdAsync(long ticketId);
    Task CreateEventAsync(CreateEventInTicketDto createDto);
    Task<List<EventDto>> GetListEventsByPeriodAsync(DateTime startPeriod, DateTime endPeriod);
    Task<List<EventDto>> GetListEventsByPeriodCurrentUserAsync(DateTime startPeriod, DateTime endPeriod);
}
