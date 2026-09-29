namespace KV.Server;
using Volo.Abp.Application.Dtos;

public class GetCustomerListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
    public string SearchStr { get; set; }
    public string SearchAddress { get; set; }
    public string SearchLongName { get; set; }
    public string SearchShortName { get; set; }
    public string SearchByActive { get; set; }
    public string SearchResponsibleManager { get; set; }
    public string SearchResponsibleManagerId { get; set; }
    public string ManagerId { get; set; }
}
