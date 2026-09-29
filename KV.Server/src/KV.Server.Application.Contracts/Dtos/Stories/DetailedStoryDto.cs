namespace KV.Server;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

public class DetailedStoryDto : AuditedEntityDto<Guid>
{
    public string CoverImageUrl { get; set; }
    public int OrderIndex { get; set; }
    public string GroupName { get; set; }
    public string Name { get; set; }
    public StoryStatus Status { get; set; }
    public List<SlideDto> Slides { get; set; }
}
