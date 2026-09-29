using Newtonsoft.Json;

namespace KV.Server.Dtos.Metadata;
public class Pagination
{
    [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
    public long? Page { get; set; }

    [JsonProperty("pageSize", NullValueHandling = NullValueHandling.Ignore)]
    public long? PageSize { get; set; }

    [JsonProperty("pageCount", NullValueHandling = NullValueHandling.Ignore)]
    public long? PageCount { get; set; }

    [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
    public long? Total { get; set; }
}
