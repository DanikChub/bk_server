namespace KV.Server;
using Volo.Abp.Application.Dtos;

public class GetAnswerTemplateListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
}
