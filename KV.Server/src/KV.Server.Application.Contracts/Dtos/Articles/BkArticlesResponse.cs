namespace KV.Server.Dtos.Articles;
using System;
using System.Collections.Generic;
using System.Globalization;
using KV.Server.Dtos.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class BkArticlesResponse
{
    [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
    public List<Datum> Data { get; set; }

    [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
    public Meta Meta { get; set; }
}

public class Datum
{
    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
    public PurpleAttributes Attributes { get; set; }
}

public class PurpleAttributes
{
    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string Title { get; set; }

    [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
    public string Text { get; set; }

    [JsonProperty("startDate")] public DateTimeOffset? StartDate { get; set; }

    [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonProperty("publishedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? PublishedAt { get; set; }

    [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)]
    public string Slug { get; set; }

    [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)]
    public Categories Categories { get; set; }

    [JsonProperty("type_material", NullValueHandling = NullValueHandling.Ignore)]
    public TypeMaterial TypeMaterial { get; set; }

    [JsonProperty("seo", NullValueHandling = NullValueHandling.Ignore)]
    public List<Seo> Seo { get; set; }
}

public class Categories
{
    [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
    public List<Dat> Data { get; set; }
}

public class Dat
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public long? Id { get; set; }

    [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
    public DataAttributes Attributes { get; set; }
}

public class DataAttributes
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonProperty("updatedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonProperty("publishedAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? PublishedAt { get; set; }

    [JsonProperty("UID", NullValueHandling = NullValueHandling.Ignore)]
    public string Uid { get; set; }
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

public class TypeMaterial
{
    [JsonProperty("data")] public Dat Data { get; set; }
}
public partial class BkArticlesResponse
{
    public static BkArticlesResponse FromJson(string json) => JsonConvert.DeserializeObject<BkArticlesResponse>(json, Converter.Settings);
}

public static class Serialize
{
    public static string ToJson(this BkArticlesResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
