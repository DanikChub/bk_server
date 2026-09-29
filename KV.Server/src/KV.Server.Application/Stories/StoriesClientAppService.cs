namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

/// <summary>
///     Обрабатывает клиентские методы сторис
/// </summary>
public class StoriesClientAppService : ApplicationService, IStoriesClientAppService
{
    private readonly IBlobContainer _blobContainer;
    private readonly IRepository<SlideItem, Guid> _slidesRepository;
    private readonly IRepository<StoryItem, Guid> _storiesRepository;

    public StoriesClientAppService(IRepository<StoryItem, Guid> storiesRepository,
        IRepository<SlideItem, Guid> slidesRepository,
        IBlobContainerFactory blobContainerFactory)
    {
        this._slidesRepository = slidesRepository;
        this._storiesRepository = storiesRepository;
        this._blobContainer = blobContainerFactory.Create(BlobContainers.STORIES);
    }

    /// <summary>
    ///     Возврящает отсортированный список историй с слайдами.
    /// </summary>
    /// <returns></returns>
    [Authorize]
    public async Task<PagedResultDto<DetailedStoryDto>> GetStoriesListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = (await this._storiesRepository.GetQueryableAsync())
            .OrderBy(input.Sorting);
        var totalCount = query.Count();
        var stories = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();
        var storiesDtos = this.ObjectMapper.Map<List<StoryItem>, List<DetailedStoryDto>>(stories);
        foreach (var story in storiesDtos)
        {
            story.Slides = await this.GetSlidesAsync(story.Id);
        }

        return new PagedResultDto<DetailedStoryDto>(totalCount, storiesDtos);
    }

    /// <summary>
    ///     Полный список всех историй
    /// </summary>
    /// <returns></returns>
    public async Task<List<DetailedStoryDto>> GetListStories()
    {
        var query = await this._storiesRepository.GetQueryableAsync();
        var stories = query.ToList();
        var storiesDtos = this.ObjectMapper.Map<List<StoryItem>, List<DetailedStoryDto>>(stories);
        foreach (var story in storiesDtos)
        {
            story.Slides = await this.GetSlidesAsync(story.Id);
        }

        return storiesDtos;
    }

    /// <summary>
    ///     Скачать историю
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    public async Task<IRemoteStreamContent> GetDownloadAsync(string id)
    {
        var content = await this._blobContainer.GetOrNullAsync(id);
        if (content == null)
        {
            throw new KeyNotFoundException("Файл не найден");
        }
        var remoteStream = new RemoteStreamContent(content, contentType: "image/jpg");
        return remoteStream;
    }

    [Authorize]
    public async Task<List<SlideDto>> GetSlidesAsync(Guid storyId)
    {
        var slides = await this._slidesRepository.GetListAsync(x => x.StoryId == storyId);
        var slDtos = this.ObjectMapper.Map<List<SlideItem>, List<SlideDto>>(slides);
        slDtos.OrderByDescending(x => x.OrderIndex);
        return slDtos;
    }

    [Authorize]
    public async Task<List<StoryDto>> GetListActiveStoryByGroupNameAsync(string name)
    {
        var normalizedName = name;
        var story = (await this._storiesRepository.GetQueryableAsync())
            .Where(x => x.GroupName == normalizedName)
            .Where(x => x.Status == StoryStatus.Active)
            .ToList();
        var dtos = this.ObjectMapper.Map<List<StoryItem>, List<StoryDto>>(story);
        return dtos;
    }
}
