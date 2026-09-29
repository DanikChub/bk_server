using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KV.Server.Dtos.DateApi;
public class CalendarDayDto
{
    [Newtonsoft.Json.JsonProperty("date", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public DateTime Date { get; set; }

    [Newtonsoft.Json.JsonProperty("type_id", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string TypeId { get; set; }

    [Newtonsoft.Json.JsonProperty("type_text", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string TypeText { get; set; }

    [Newtonsoft.Json.JsonProperty("note", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Note { get; set; }

    [Newtonsoft.Json.JsonProperty("week_day", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string WeekDay { get; set; }

    [Newtonsoft.Json.JsonProperty("working_hours", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public int WorkingHours { get; set; }
}
