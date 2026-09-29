using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KV.Server.Dtos.DateApi;
public class ProductionCalendarDto
{
    [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Status { get; set; }

    [Newtonsoft.Json.JsonProperty("country_code", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string CountryCode { get; set; }

    [Newtonsoft.Json.JsonProperty("country_text", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string CountryText { get; set; }

    [Newtonsoft.Json.JsonProperty("dt_start", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public DateTime Start { get; set; }

    [Newtonsoft.Json.JsonProperty("dt_end", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public DateTime End { get; set; }

    [Newtonsoft.Json.JsonProperty("data", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string WorkWeekType { get; set; }

    [Newtonsoft.Json.JsonProperty("period", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Period { get; set; }

    [Newtonsoft.Json.JsonProperty("days", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public List<CalendarDayDto> Days { get; set; }
}
