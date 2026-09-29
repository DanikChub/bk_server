namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Permissions.Stories;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

/// <summary>
///     Сервис обрабатывающий добавление\удаление историй и слайдов историй,
///     сохраняющий изображения в контейнере.
/// </summary>
[Authorize(StoriesPermissions.GroupName)]
public class StoriesManagementAppService : ApplicationService, IStoriesManagementAppService
{
    private readonly IBlobContainer _blobContainer;
    private readonly IRepository<SlideItem, Guid> _slidesRepository;
    private readonly IRepository<StoryItem, Guid> _storiesRepository;

    public StoriesManagementAppService(IRepository<StoryItem, Guid> storiesRepository,
        IRepository<SlideItem, Guid> slidesRepository,
        IBlobContainerFactory blobContainerFactory)
    {
        this._slidesRepository = slidesRepository;
        this._storiesRepository = storiesRepository;
        this._blobContainer = blobContainerFactory.Create(BlobContainers.STORIES);
    }

    /// <summary>
    ///     Создает историю в сущности StoryItem.
    /// </summary>
    /// <param name="story"> Параметры истории </param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Create)]
    public async Task<StoryDto> CreateStoryAsync(CreateUpdateStoryDto story)
    {
        var storyItem = new StoryItem(Guid.NewGuid(), story.CoverImageUrl, story.OrderIndex, story.GroupName, story.Name, story.Status);
        await this._storiesRepository.InsertAsync(storyItem);
        return this.ObjectMapper.Map<StoryItem, StoryDto>(storyItem);
    }

    /// <summary>
    ///     Обновляет историю.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="story"></param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Edit)]
    public async Task<StoryDto> UpdateStoryAsync(CreateUpdateStoryDto story)
    {
        var storyItem = await this._storiesRepository.GetAsync(story.Id);
        storyItem.CoverImageUrl = story.CoverImageUrl;
        storyItem.OrderIndex = story.OrderIndex;
        storyItem.GroupName = story.GroupName;
        storyItem.Name = story.Name;
        storyItem.Status = story.Status;
        await this._storiesRepository.UpdateAsync(storyItem);
        return this.ObjectMapper.Map<StoryItem, StoryDto>(storyItem);
    }

    /// <summary>
    ///     Создает слайд истории в сущности SlideItem связанный с указанной историей
    /// </summary>
    /// <param name="slide"></param>
    /// <param name="storyId"></param>
    /// <param name="orderIndex"></param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Create)]
    public async Task CreateSlideAsync(CreateUpdateSlideDto slide)
    {
        var sl = new SlideItem(Guid.NewGuid(), slide.StoryId, slide.ImageUrl, slide.OrderIndex);
        await this._slidesRepository.InsertAsync(sl);
    }

    /// <summary>
    ///     Обновляет слайд.
    /// </summary>
    /// <param name="slide"></param>
    /// <param name="storyId"></param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Edit)]
    public async Task<SlideDto> UpdateSlideAsync(Guid id, CreateUpdateSlideDto slide)
    {
        var sl = await this._slidesRepository.GetAsync(id);
        sl.StoryId = slide.StoryId;
        sl.ImageUrl = slide.ImageUrl;
        sl.OrderIndex = slide.OrderIndex;
        await this._slidesRepository.UpdateAsync(sl);
        return this.ObjectMapper.Map<SlideItem, SlideDto>(sl);
    }

    /// <summary>
    ///     Возвращает историю.
    /// </summary>
    /// <param name="id"> Идентификатор сущности StoryItem  </param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Get)]
    public async Task<StoryDto> GetStoryAsync(Guid id)
    {
        var st = await this._storiesRepository.GetAsync(id);
        return this.ObjectMapper.Map<StoryItem, StoryDto>(st);
    }

    /// <summary>
    ///     Удаляет историю.
    /// </summary>
    /// <param name="id"> Идентификатор сущности StoryItem </param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Delete)]
    public async Task DeleteStoryAsync(Guid id)
    {
        var sllides = await this.GetSlidesAsync(id);
        foreach (var sl in sllides)
        {
            if (await this._blobContainer.GetOrNullAsync(sl.Id.ToString()) != null)
            {
                await this._blobContainer.DeleteAsync(sl.Id.ToString());
            }
        }

        if (await this._blobContainer.GetOrNullAsync(id.ToString()) != null)
        {
            await this._blobContainer.DeleteAsync(id.ToString());
        }

        await this._storiesRepository.DeleteAsync(id);
    }

    /// <summary>
    ///     Удаляет слайд.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Delete)]
    public async Task DeleteSlideAsync(Guid id)
    {
        await this._slidesRepository.DeleteAsync(id);
        if (await this._blobContainer.GetOrNullAsync(id.ToString()) != null)
        {
            await this._blobContainer.DeleteAsync(id.ToString());
        }
    }

    /// <summary>
    ///     Возвращает список историй.
    /// </summary>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Get)]
    public async Task<PagedResultDto<StoryDto>> GetStoriesListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await this._storiesRepository.GetQueryableAsync();
        query = query.OrderBy(input.Sorting);
        var totalCount = query.Count();
        var products = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount == 0 ? LimitedResultRequestDto.DefaultMaxResultCount : input.MaxResultCount)
            .ToList();
        var productDtos = this.ObjectMapper.Map<List<StoryItem>, List<StoryDto>>(products);
        return new PagedResultDto<StoryDto>(totalCount, productDtos);
    }

    /// <summary>
    ///     Возвращает список слайдов истории.
    /// </summary>
    /// <param name="storyId"> Идентификатор сущности StoryItem </param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Get)]
    public async Task<List<SlideDto>> GetSlidesAsync(Guid storyId)
    {
        var slides = await this._slidesRepository.GetListAsync(x => x.StoryId == storyId);
        var slDtos = this.ObjectMapper.Map<List<SlideItem>, List<SlideDto>>(slides);
        return slDtos;
    }

    /// <summary>
    ///     Возвращает список слайдеров.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Get)]
    public async Task<PagedResultDto<SlideDto>> GetSlidersListAsync(GetSlidersListRequestDto input)
    {
        var query = await this._slidesRepository.GetQueryableAsync();
        query = query.Where(x => x.StoryId == input.StoryId);
        if (!input.Sorting.IsNullOrWhiteSpace())
        {
            query = query.OrderBy(input.Sorting);
        }

        var totalCount = query.Count();
        var sliders = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount == 0 ? LimitedResultRequestDto.DefaultMaxResultCount : input.MaxResultCount)
            .ToList();
        var dtos = this.ObjectMapper.Map<List<SlideItem>, List<SlideDto>>(sliders);
        return new PagedResultDto<SlideDto>(totalCount, dtos);
    }

    /// <summary>
    ///     Загружает файл в контейнер. Возвращает DTO с информацией для скачивания файла.
    /// </summary>
    /// <param name="id"> Имя файла в контейнере.</param>
    /// <param name="streamContent"></param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Create)]
    public async Task<UploadResultDto> UploadAsync(IRemoteStreamContent streamContent)
    {
        var id = Guid.NewGuid();
        await this._blobContainer.SaveAsync(id.ToString(), streamContent.GetStream());
        return new UploadResultDto
        {
            Id = id
        };
    }

    /// <summary>
    ///     Полный список всех историй
    /// </summary>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Get)]
    public async Task<List<DetailedStoryDto>> GetListStoriesAsync()
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
    ///     Получает слайдер по id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Get)]
    public async Task<SlideDto> GetSliderBySliderIdAsync(Guid id)
    {
        var slider = await this._slidesRepository.FirstOrDefaultAsync(x => x.Id == id);
        var dto = this.ObjectMapper.Map<SlideItem, SlideDto>(slider);
        return dto;
    }

    /// <summary>
    ///     Скачивает файл из контейнера по указанному имени.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [Authorize(StoriesPermissions.Stories.Get)]
    public async Task<IRemoteStreamContent> GetDownloadAsync(string id) => new RemoteStreamContent(await this._blobContainer.GetOrNullAsync(id), contentType: "image/jpg");
}
