using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace KV.Server.Dtos.Employees;
public class GetEmployeeProfileListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
    public string SearchFirstName { get; set; }
    public string SearchLastName { get; set; }
    public string SearchMiddleName { get; set; }
    public string SearchDateOfBirthDay { get; set; }
    public string SearchJobPost { get; set; }
}