namespace KV.Server;
using System;

public class CreateUpdateSlideDto
{
    public Guid StoryId { get; set; }
    public string ImageUrl { get; set; }
    public int OrderIndex { get; set; }
}
