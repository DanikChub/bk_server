namespace KV.Server.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Permissions.Tickets;
using KV.Server.Tickets;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

[Authorize(TicketPermissions.Tickets.Default)]
public class TagsAppService : ApplicationService, ITagsAppService
{
    private readonly IRepository<Tag, Guid> _tagRepository;
    private readonly IRepository<TicketTag> _ticketTagRepository;

    public TagsAppService(IRepository<Tag, Guid> tagRepository,
        IRepository<TicketTag> ticketTagRepository)
    {
        this._tagRepository = tagRepository;
        this._ticketTagRepository = ticketTagRepository;
    }

    [Authorize(TicketPermissions.Tickets.Get)]
    public async Task<List<TagDto>> GetPopularByTicketTagAsync()
    {
        var tags = await this._tagRepository.ToListAsync();
        var dtos = this.ObjectMapper.Map<List<Tag>, List<TagDto>>(tags);
        return dtos;
    }

    [Authorize(TicketPermissions.Tickets.Create)]
    public async Task<TagDto> CreateTagAsync(CreateUpdateTagDto createDto)
    {
        var tag = this.ObjectMapper.Map<CreateUpdateTagDto, Tag>(createDto);
        var entity = await this._tagRepository.InsertAsync(tag);
        var dto = this.ObjectMapper.Map<Tag, TagDto>(entity);
        return dto;
    }

    [Authorize(TicketPermissions.Tickets.Delete)]
    public async Task DeleteTagInTicketAsync(long ticketId, Guid tagId) => await this._ticketTagRepository.DeleteAsync(x => x.TicketId == ticketId && x.TagId == tagId);

    [Authorize(TicketPermissions.Tickets.Create)]
    public async Task<TagDto> InsertTagInTicketAsync(CreateUpdateTicketTagDto createDto)
    {
        var tag = await this._tagRepository.FirstOrDefaultAsync(x => x.Name == createDto.TagName);
        tag ??= await this._tagRepository.InsertAsync(new Tag
        {
            Name = createDto.TagName
        });

        var ticketTag =
            await this._ticketTagRepository.FirstOrDefaultAsync(x => x.TicketId == createDto.TicketId && x.TagId == tag.Id);
        if (ticketTag == null)
        {
            await this._ticketTagRepository.InsertAsync(new TicketTag
            {
                TagId = tag.Id,
                TicketId = createDto.TicketId
            });
        }

        var dto = this.ObjectMapper.Map<Tag, TagDto>(tag);
        return dto;
    }

}
