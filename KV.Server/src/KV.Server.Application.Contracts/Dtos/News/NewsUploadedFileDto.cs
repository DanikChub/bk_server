namespace KV.Server.Dtos.News;
using System;

public class NewsUploadedFileDto
{
    public long? Id { get; set; }
    public long? Width { get; set; }
    public long? Height { get; set; }
    public long? Size { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public string ContentType { get; set; }
    public Uri Url { get; set; }
    public DateTimeOffset? CreationTime { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
}
