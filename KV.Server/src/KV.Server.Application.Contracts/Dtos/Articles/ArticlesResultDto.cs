namespace KV.Server.Dtos.Articles;
using System;

public class ArticlesResultDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public string Slug { get; set; }
    public string CategoryName { get; set; }
    public string TypeMaterial { get; set; }
    public DateTimeOffset? CreationTime { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string[] Seo { get; set; }
}
