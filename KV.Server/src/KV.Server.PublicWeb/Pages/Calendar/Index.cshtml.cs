namespace KV.Server.PublicWeb.Pages.Calendar;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class IndexModel : ServerPageModel
{
    private readonly IWorkCalendarAppService _dateApiAppService;

    public IndexModel(IWorkCalendarAppService dateApiAppService)
    {
        this._dateApiAppService = dateApiAppService;
    }

    public DateCountDto DateCountDto { get; set; } = new();

    public async Task OnGetAsync()
    {
        var startDate = DateTime.ParseExact($"{Clock.Now.Year}-{Clock.Now:MM}-01", "yyyy-MM-dd",
            CultureInfo.InvariantCulture);
        var endDate =   startDate.AddDays(DateTime.DaysInMonth(startDate.Year, startDate.Month) - startDate.Day);
        this.DateCountDto = await this._dateApiAppService
            .GetCountDaysAsync(startDate, endDate);
    }
}
