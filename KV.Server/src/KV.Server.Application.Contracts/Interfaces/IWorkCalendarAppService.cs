namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface IWorkCalendarAppService : IApplicationService
{
    Task<DateCountDto> GetCountDaysAsync(DateTime startDate, DateTime endDate);
    Task<List<DateTime>> GetHolidaysAsync(DateTime startDate, DateTime endDate);
}
