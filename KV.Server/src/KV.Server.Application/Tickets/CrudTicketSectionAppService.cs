namespace KV.Server.Tickets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Permissions.TicketSections;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

public class CrudTicketSectionAppService :
    CrudAppService<TicketSection, TicketSectionDto, Guid, GetTicketSectionListRequestDto, CreateUpdateTicketSectionDto>,
    ICrudTicketSectionAppService
{
    public CrudTicketSectionAppService(IRepository<TicketSection, Guid> repository) : base(repository)
    {
    }

    [Authorize(TicketSectionPermissions.TicketSections.Create)]
    public override Task<TicketSectionDto> CreateAsync(CreateUpdateTicketSectionDto input) => this.TryDisableMultiTenantAsync(() => base.CreateAsync(input));

    [Authorize(TicketSectionPermissions.TicketSections.Get)]
    public override Task<TicketSectionDto> GetAsync(Guid id) => this.TryDisableMultiTenantAsync(() => base.GetAsync(id));

    [Authorize(TicketSectionPermissions.TicketSections.Get)]
    public override Task<PagedResultDto<TicketSectionDto>> GetListAsync(GetTicketSectionListRequestDto input) => this.TryDisableMultiTenantAsync(() => base.GetListAsync(input));

    [Authorize(TicketSectionPermissions.TicketSections.Delete)]
    public override Task DeleteAsync(Guid id) => this.TryDisableMultiTenantAsync(() => base.DeleteAsync(id));

    [Authorize(TicketSectionPermissions.TicketSections.Edit)]
    public override Task<TicketSectionDto> UpdateAsync(Guid id, CreateUpdateTicketSectionDto input) => this.TryDisableMultiTenantAsync(() => base.UpdateAsync(id, input));

    public async Task<List<TicketSectionDto>> GetListTicketSectionsAsync()
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var entities = await this.Repository.ToListAsync();
            return this.ObjectMapper.Map<List<TicketSection>, List<TicketSectionDto>>(entities);
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
