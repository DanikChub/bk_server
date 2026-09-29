namespace KV.Server;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface IArticleAppService : IApplicationService
{
    public Task<PagedResultDto<ArticlesDto>> GetArticleListAsync(PagedAndSortedResultRequestDto input);
}
