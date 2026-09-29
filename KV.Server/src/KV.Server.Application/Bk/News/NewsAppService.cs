namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KV.Server.Application.OpenAPIs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

/// <summary>
///     Сервис для получения новостей с сайта https://bk.kv34.ru/
/// </summary>
[Authorize]
public class NewsAppService : ApplicationService, INewsAppService
{
    private readonly BkOpenApi _bkOpenApi; // поле подключения к стороннему серверу 

    //через которую можно вызывать описанные в yaml файле методы, News и Article
    private readonly IConfiguration _config;

    public NewsAppService(IHttpClientFactory cFactory, IConfiguration config)
    {
        this._bkOpenApi = new BkOpenApi(cFactory.CreateClient());
        this._config = config;
    }

    /// <summary>
    ///     Возвращает пагинированный список новостей
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [Authorize]
    public async Task<PagedResultDto<NewsDto>> GetNewsListAsync(PagedAndSortedResultRequestDto input)
    {
        var newsList = new List<NewsDto>();
        int pageIndex;
        if (input.MaxResultCount > 50) // поскольку bk возвращает не больше 50 на странице
        {
            input.MaxResultCount = 50; // возвращаеем не больше 50
        }

        pageIndex = (input.SkipCount / input.MaxResultCount) + 1;
        var l = "title,thumbnail,searchSummary,_purpose,publishedAt"; // выбираем атирубы для парсинга
        var requestResult = await this._bkOpenApi.NewsAsync(this._config["BkOpenApi:NewsKey"]
            , l, true, input.MaxResultCount, pageIndex);

        var totalCount = requestResult.Total;

        foreach (var item in requestResult.Results)
        {
            var purposes = new List<string>();
            foreach (var purpose in item._purpose)
            {
                purposes.Add(purpose.Title);
            }

            var ns = new NewsDto
            {
                Name = item.Title,
                Url = item._url,
                ShortContent = item.SearchSummary,
                CoverUrls = item.Thumbnail.Items.First()._pieces.First().Item.Attachment._urls.Full,
                Purpose = purposes,
                PublishedAt = ConvertPublishedDate(item.PublishedAt, "yyyy-MM-dd")
            };
            newsList.Add(ns);
        }

        return new PagedResultDto<NewsDto>(totalCount, newsList);
    }

    private static DateTime ConvertPublishedDate(string input, string format)
    {
        DateTime.TryParseExact(input, format,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt);
        return dt;
    }
}
