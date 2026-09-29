namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class StoryDto : AuditedEntityDto<Guid>
{
    public string CoverImageUrl { get; set; }
    public int OrderIndex { get; set; }
    public string GroupName { get; set; }
    public string Name { get; set; }
    public StoryStatus Status { get; set; }
}
