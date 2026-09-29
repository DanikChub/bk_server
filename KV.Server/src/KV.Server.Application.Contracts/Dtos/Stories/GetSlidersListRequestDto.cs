namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class GetSlidersListRequestDto : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
    public Guid StoryId { get; set; }
}
