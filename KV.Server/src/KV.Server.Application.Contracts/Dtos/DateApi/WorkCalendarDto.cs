using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KV.Server.Dtos.DateApi;
public class WorkCalendarDto
{
    public List<DateTime> WorkDays { get; set; } = new();
    public List<DateTime> Holidays { get; set; } = new();
}
