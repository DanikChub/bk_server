namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class ConstraintTypeDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public int NowCount { get; set; }
    public int MaxCount { get; set; }
}
