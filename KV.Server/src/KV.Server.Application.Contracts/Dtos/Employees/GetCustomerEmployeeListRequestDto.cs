namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class GetCustomerEmployeeListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
    public Guid CustomerId { get; set; }
    public string SearchFirstName { get; set; }
    public string SearchLastName { get; set; }
    public string SearchMiddleName { get; set; }
    public string SearchDateOfBirthDay { get; set; }
    public string SearchJobPost { get; set; }
}
