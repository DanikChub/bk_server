namespace KV.Server.Dtos.News;
using System;
using System.Collections.Generic;
using System.Globalization;
using KV.Server.Dtos.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class BkNewsResponse
{
    [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
    public List<NewsResultDto> Data { get; set; }

    [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
    public Meta Meta { get; set; }
}

public class NewsResultDto
{
    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
    public PurpleAttributes Attributes { get; set; }
}

public class PurpleAttributes
{
    [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonProperty("publishedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? PublishedAt { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string Title { get; set; }

    [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)]
    public string Slug { get; set; }

    [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
    public string Body { get; set; }

    [JsonProperty("regions", NullValueHandling = NullValueHandling.Ignore)]
    public Ions Regions { get; set; }

    [JsonProperty("directions", NullValueHandling = NullValueHandling.Ignore)]
    public Ions Directions { get; set; }

    [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
    public Image Image { get; set; }

    [JsonProperty("seo", NullValueHandling = NullValueHandling.Ignore)]
    public Seo Seo { get; set; }
}

public class Ions
{
    [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
    public List<DirectionsDatum> Data { get; set; }
}

public class DirectionsDatum
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public long? Id { get; set; }

    [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
    public FluffyAttributes Attributes { get; set; }
}

public class FluffyAttributes
{
    [JsonProperty("UID", NullValueHandling = NullValueHandling.Ignore)]
    public string Uid { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonProperty("publishedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? PublishedAt { get; set; }
}

public class Image
{
    [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
    public List<ImageDatum> Data { get; set; }
}

public class ImageDatum
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public long? Id { get; set; }

    [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
    public TentacledAttributes Attributes { get; set; }
}

public class TentacledAttributes
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("alternativeText")] public object AlternativeText { get; set; }

    [JsonProperty("caption")] public object Caption { get; set; }

    [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
    public long? Width { get; set; }

    [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
    public long? Height { get; set; }

    [JsonProperty("formats", NullValueHandling = NullValueHandling.Ignore)]
    public Formats Formats { get; set; }

    [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
    public string Hash { get; set; }

    [JsonProperty("ext", NullValueHandling = NullValueHandling.Ignore)]
    public string Ext { get; set; }

    [JsonProperty("mime", NullValueHandling = NullValueHandling.Ignore)]
    public string Mime { get; set; }

    [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
    public double? Size { get; set; }

    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public Uri Url { get; set; }

    [JsonProperty("previewUrl")] public object PreviewUrl { get; set; }

    [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
    public string Provider { get; set; }

    [JsonProperty("provider_metadata")] public object ProviderMetadata { get; set; }

    [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class Formats
{
    [JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
    public Thumbnail Thumbnail { get; set; }
}

public class Thumbnail
{
    [JsonProperty("ext", NullValueHandling = NullValueHandling.Ignore)]
    public string Ext { get; set; }

    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public Uri Url { get; set; }

    [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
    public string Hash { get; set; }

    [JsonProperty("mime", NullValueHandling = NullValueHandling.Ignore)]
    public string Mime { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("path")] public object Path { get; set; }

    [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
    public double? Size { get; set; }

    [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
    public long? Width { get; set; }

    [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
    public long? Height { get; set; }
}

public class Seo
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public long? Id { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string Title { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty("keywords", NullValueHandling = NullValueHandling.Ignore)]
    public string Keywords { get; set; }
}

public partial class BkNewsResponse
{
    public static BkNewsResponse FromJson(string json) => JsonConvert.DeserializeObject<BkNewsResponse>(json, Converter.Settings);
}

public static class Serialize
{
    public static string ToJson(this BkNewsResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new()
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        }
    };
}
