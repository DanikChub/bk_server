namespace KV.Server;

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using KV.Server.Dtos.Articles;
using KV.Server.Dtos.News;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public class PostsAppService : ApplicationService, IPostsAppService
{
    public const string AuthorizationName = "Authorization";

    public const string ArticlesQueryUrl =
        @"articles?sort[0]=createdAt%3Adesc&pagination[page]={0}&pagination[pageSize]={1}&populate[0]=categories&populate[1]=type_material&populate[2]=seo";

    public const string NewsQueryUrl =
        @"news?sort[0]=publishedAt%3Aasc&pagination[page]={0}&pagination[pageSize]={1}&populate[0]=regions&populate[1]=directions&populate[2]=image&populate[3]=seo";

    private readonly ILogger<PostsAppService> _logger;

    private readonly string _mainUrl;
    private readonly string _authorizationValue;

    public PostsAppService(IConfiguration configuration, ILogger<PostsAppService> logger)
    {
        _logger = logger;
        _mainUrl = configuration["BkOpenApiConfig:BaseUrl"];
        _authorizationValue = configuration["BkOpenApiConfig:AuthorizationValue"];
    }

    [Authorize]
    public async Task<PagedResultDto<ArticlesResultDto>> GetArticlesAsync(int page, int take = 5)
    {
        PagedResultDto<ArticlesResultDto> result;

        try
        {
            var response = await GetItemsAsync<BkArticlesResponse>(ArticlesQueryUrl, page, take);
            var dtos = this.ObjectMapper.Map<List<Datum>, List<ArticlesResultDto>>(response.Data);
            return new PagedResultDto<ArticlesResultDto>(response.Data.Count, dtos);
        }
        catch (JsonSerializationException ex)
        {
            _logger.LogError(ex, ex.Message);
            return new PagedResultDto<ArticlesResultDto>(0, new List<ArticlesResultDto>());
        }

        throw new UserFriendlyException("Error Response");
    }

    [Authorize]
    public async Task<PagedResultDto<NewsResultDto>> GetNewsAsync(int page, int take = 5)
    {
        PagedResultDto<NewsResultDto> result;

        try
        {
            var response = await GetItemsAsync<BkNewsResponse>(NewsQueryUrl, page, take);
            return new PagedResultDto<NewsResultDto>(response.Data.Count, response.Data);

        }
        catch (JsonSerializationException ex)
        {
            _logger.LogError(ex, ex.Message);
            return new PagedResultDto<NewsResultDto>(0, new List<NewsResultDto>());
        }

        throw new UserFriendlyException("Error Response");
    }

    private async Task<TResponse> GetItemsAsync<TResponse>(string url, int page, int take)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add(AuthorizationName, _authorizationValue);
            var query = string.Format(CultureInfo.InvariantCulture, url, page, take);
            var uri = $"{_mainUrl}{query}";
            using (var response = await client.GetAsync(uri))
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var deserializedResponse = JsonConvert.DeserializeObject<TResponse>(responseBody);
                return deserializedResponse;
            }
        }
    }
}
