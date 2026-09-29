namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KV.Server.Dtos.DateApi;
using KV.Server.Permissions.Calendars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;

public class WorkCalendarAppService : ApplicationService, IWorkCalendarAppService
{
    public const string UrlApiHolidays = "https://production-calendar.ru/get-period";
    private readonly IDistributedCache<WorkCalendarDto, string> _cache;
    private readonly IConfiguration _configuration;

    public WorkCalendarAppService(IDistributedCache<WorkCalendarDto, string> cache, IConfiguration configuration)
    {
        _cache = cache;
        _configuration = configuration;
    }

    [Authorize(CalendarPermissions.Calendars.Get)]
    public async Task<DateCountDto> GetCountDaysAsync(DateTime startDate, DateTime endDate)
    {
        var calendar = await GetOrAddCalendarCacheAsync(startDate);

        var result = new DateCountDto
        {
            CountHolidays = calendar.Holidays
            .Where(x => (x.Date >= startDate.Date && x.Date <= endDate.Date) && x.Date.DayOfWeek != DayOfWeek.Saturday && x.Date.DayOfWeek != DayOfWeek.Sunday)
            .Count(),
            CountWeekends = calendar.Holidays
            .Where(x => (x.Date >= startDate.Date && x.Date <= endDate.Date) && (x.Date.DayOfWeek == DayOfWeek.Saturday || x.Date.DayOfWeek == DayOfWeek.Sunday))
            .Count(),
            CountWorkdays = calendar.WorkDays
            .Where(x => x.Date >= startDate.Date && x.Date <= endDate.Date)
            .Count(),
            CountDays = Convert.ToInt32((endDate - startDate).TotalDays + 1)
        };
        return result;
    }

    public async Task<List<DateTime>> GetHolidaysAsync(DateTime startDate, DateTime endDate)
    {
        var calendar = await GetOrAddCalendarCacheAsync(startDate);

        return calendar.Holidays.Where(x => x.Date >= startDate.Date && x.Date <= endDate.Date).ToList();
    }

    private async Task<WorkCalendarDto> GetOrAddCalendarCacheAsync(DateTime startDate)
    {
        return await _cache.GetOrAddAsync(
                $"WorkCalendar_{startDate.Year}",
                async () =>
                {
                    return await GetWorkCalendarAsync(startDate.Year);

                },
                () => new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddHours((new DateTime(startDate.Year + 1, 1, 1, 0, 0, 0) - startDate).TotalHours)

                }
            );
    }

    private async Task<WorkCalendarDto> GetWorkCalendarAsync(int year)
    {
        var token = _configuration["WorkCalendar:Token"];

        using (var httpClient = new HttpClient())
        {
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            var response = await httpClient.GetAsync($"{UrlApiHolidays}/{token}/ru/{year}/json");
            var jsonString = await response.Content.ReadAsStringAsync();

            var productionCalendar = JsonConvert.DeserializeObject<ProductionCalendarDto>(jsonString, new JsonSerializerSettings() { DateFormatString = "dd.MM.yyyy", DateTimeZoneHandling = DateTimeZoneHandling.Utc });

            var calendarResult = new WorkCalendarDto();

            foreach (var day in productionCalendar.Days)
            {
                if (day.WorkingHours > 0)
                {
                    calendarResult.WorkDays.Add(day.Date);
                }
                else
                {
                    calendarResult.Holidays.Add(day.Date);
                }
            }

            return calendarResult;
        }
    }
}
