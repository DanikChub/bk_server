namespace KV.Server;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface INewsAppService : IApplicationService
{
    public Task<PagedResultDto<NewsDto>> GetNewsListAsync(PagedAndSortedResultRequestDto input);
}
