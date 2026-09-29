namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using KV.Server.Application.OpenAPIs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

/// <summary>
///     Сервис для получения статей с сайта https://bk.kv34.ru/
/// </summary>
[Authorize]
public class ArticlesAppService : ApplicationService, IArticleAppService
{
    private readonly BkOpenApi _bkOpenApi; // поле подключения к стороннему

    //серверу 
    //через которую можно вызывать описанные в yaml файле методы, News и Article
    private readonly IConfiguration _config;

    public ArticlesAppService(IHttpClientFactory cFactory, IConfiguration config)
    {
        this._bkOpenApi = new BkOpenApi(cFactory.CreateClient());
        this._config = config;
    }

    /// <summary>
    ///     Возвращает пагинированный список статей.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [Authorize]
    public async Task<PagedResultDto<ArticlesDto>> GetArticleListAsync(PagedAndSortedResultRequestDto input)
    {
        var articleList = new List<ArticlesDto>();
        int pageIndex;
        if (input.MaxResultCount > 50) // поскольку bk возвращает не больше 50 на странице
        {
            input.MaxResultCount = 50; // возвращаеем не больше 50
        }

        pageIndex = (input.SkipCount / input.MaxResultCount) + 1;
        var l = "title,postType,publishedAt"; // выбираем атирубы для парсинга
        var requestResult = await this._bkOpenApi.ApostropheBlogAsync(this._config["BkOpenApi:ArticleKey"]
            , l, true, input.MaxResultCount, pageIndex);

        var totalCount = requestResult.Total;

        foreach (var item in requestResult.Results)
        {
            var ar = new ArticlesDto
            {
                Name = item.Title,
                Url = item._url,
                Type = item.PostType,
                PublishedAt = ConvertPublishedDate(item.PublishedAt, "yyyy-MM-dd")
            };
            articleList.Add(ar);
        }

        return new PagedResultDto<ArticlesDto>(totalCount, articleList);
    }

    private static DateTime ConvertPublishedDate(string input, string format)
    {
        DateTime.TryParseExact(input, format,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt);
        return dt;
    }
}

// yyyy-mm-dd
