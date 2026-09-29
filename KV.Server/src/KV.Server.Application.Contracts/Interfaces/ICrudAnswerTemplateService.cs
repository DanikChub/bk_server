namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface ICrudAnswerTemplateService : ICrudAppService<AnswerTemplateDto, Guid, GetAnswerTemplateListRequestDto,
    CreateUpdateAnswerTemplateDto>
{
    Task<List<AnswerTemplateDto>> ToListAsync();
}
