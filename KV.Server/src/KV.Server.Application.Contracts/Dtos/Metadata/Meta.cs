using Newtonsoft.Json;

namespace KV.Server.Dtos.Metadata;
public class Meta
{
    [JsonProperty("pagination", NullValueHandling = NullValueHandling.Ignore)]
    public Pagination Pagination { get; set; }
}
