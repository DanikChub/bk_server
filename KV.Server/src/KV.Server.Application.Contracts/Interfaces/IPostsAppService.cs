namespace KV.Server;
using System.Threading.Tasks;
using KV.Server.Dtos.Articles;
using KV.Server.Dtos.News;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface IPostsAppService : IApplicationService
{
    Task<PagedResultDto<ArticlesResultDto>> GetArticlesAsync(int page, int take = 5);
    Task<PagedResultDto<NewsResultDto>> GetNewsAsync(int page, int take = 5);
}
