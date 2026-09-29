namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class SlideDto : EntityDto<Guid>
{
    public Guid StoryId { get; set; }
    public string ImageUrl { get; set; }
    public int OrderIndex { get; set; }
}
