namespace KV.Server;
using System;
using Volo.Abp.Application.Dtos;

public class AnswerTemplateDto : EntityDto<Guid>
{
    public string Title { get; set; }
    public string Description { get; set; }
}
