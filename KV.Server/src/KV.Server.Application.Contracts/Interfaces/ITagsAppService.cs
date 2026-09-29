namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface ITagsAppService : IApplicationService
{
    Task<TagDto> CreateTagAsync(CreateUpdateTagDto createDto);
    Task DeleteTagInTicketAsync(long ticketId, Guid tagId);
    Task<TagDto> InsertTagInTicketAsync(CreateUpdateTicketTagDto createDto);
    Task<List<TagDto>> GetPopularByTicketTagAsync();
}
