namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

/// <summary>
///     Интерфейс для сервиса StoriesAppService
/// </summary>
public interface IStoriesManagementAppService : IApplicationService
{
    Task<PagedResultDto<StoryDto>> GetStoriesListAsync(PagedAndSortedResultRequestDto input);
    Task<StoryDto> CreateStoryAsync(CreateUpdateStoryDto story);
    Task<StoryDto> UpdateStoryAsync(CreateUpdateStoryDto story);
    Task<StoryDto> GetStoryAsync(Guid id);
    Task DeleteStoryAsync(Guid id);
    Task CreateSlideAsync(CreateUpdateSlideDto slide);
    Task<SlideDto> UpdateSlideAsync(Guid id, CreateUpdateSlideDto slide);
    Task DeleteSlideAsync(Guid id);
    Task<List<SlideDto>> GetSlidesAsync(Guid storyId);
    Task<UploadResultDto> UploadAsync(IRemoteStreamContent streamContent);
    Task<IRemoteStreamContent> GetDownloadAsync(string id);
    Task<List<DetailedStoryDto>> GetListStoriesAsync();
    Task<SlideDto> GetSliderBySliderIdAsync(Guid id);
    Task<PagedResultDto<SlideDto>> GetSlidersListAsync(GetSlidersListRequestDto input);
}
