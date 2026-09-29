namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Permissions;
using KV.Server.Templates;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

public class CrudAnswerTemplateService :
    CrudAppService<AnswerTemplate, AnswerTemplateDto, Guid, GetAnswerTemplateListRequestDto,
        CreateUpdateAnswerTemplateDto>,
    ICrudAnswerTemplateService
{
    public CrudAnswerTemplateService(IRepository<AnswerTemplate, Guid> repository) : base(repository)
    {
    }

    [Authorize(AnswerTemplatePermissions.AnswerTemplates.Create)]
    public override Task<AnswerTemplateDto> CreateAsync(CreateUpdateAnswerTemplateDto input) => this.TryDisableMultiTenantAsync(() => base.CreateAsync(input));

    [Authorize(AnswerTemplatePermissions.AnswerTemplates.Get)]
    public override Task<AnswerTemplateDto> GetAsync(Guid id) => this.TryDisableMultiTenantAsync(() => base.GetAsync(id));

    [Authorize(AnswerTemplatePermissions.AnswerTemplates.Get)]
    public override Task<PagedResultDto<AnswerTemplateDto>> GetListAsync(GetAnswerTemplateListRequestDto input) => this.TryDisableMultiTenantAsync(() => base.GetListAsync(input));

    [Authorize(AnswerTemplatePermissions.AnswerTemplates.Delete)]
    public override Task DeleteAsync(Guid id) => this.TryDisableMultiTenantAsync(() => base.DeleteAsync(id));

    [Authorize(AnswerTemplatePermissions.AnswerTemplates.Edit)]
    public override Task<AnswerTemplateDto> UpdateAsync(Guid id, CreateUpdateAnswerTemplateDto input) => this.TryDisableMultiTenantAsync(() => base.UpdateAsync(id, input));

    public async Task<List<AnswerTemplateDto>> ToListAsync()
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var entities = await this.Repository.ToListAsync();
            return this.ObjectMapper.Map<List<AnswerTemplate>, List<AnswerTemplateDto>>(entities);
        }
    }

    private async Task<T> TryDisableMultiTenantAsync<T>(Func<Task<T>> func)
    {
        if (this.CurrentTenant.Id is null)
        {
            using (this.DataFilter.Disable<IMultiTenant>())
            {
                return await func();
            }
        }

        return await func();
    }

    private async Task TryDisableMultiTenantAsync(Func<Task> func)
    {
        if (this.CurrentTenant.Id is null)
        {
            using (this.DataFilter.Disable<IMultiTenant>())
            {
                await func();
            }
        }

        await func();
    }
}
