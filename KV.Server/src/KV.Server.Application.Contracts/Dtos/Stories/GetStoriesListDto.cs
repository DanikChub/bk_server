namespace KV.Server;
using Volo.Abp.Application.Dtos;

public class GetStoriesListRequestDto : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
