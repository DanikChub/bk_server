namespace KV.Server;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

public interface IStoriesClientAppService : IApplicationService
{
    Task<PagedResultDto<DetailedStoryDto>> GetStoriesListAsync(PagedAndSortedResultRequestDto input);
    Task<IRemoteStreamContent> GetDownloadAsync(string id);
    Task<List<DetailedStoryDto>> GetListStories();
    Task<List<StoryDto>> GetListActiveStoryByGroupNameAsync(string name);
    Task<List<SlideDto>> GetSlidesAsync(Guid storyId);
}
