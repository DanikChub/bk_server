namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Permissions.Contracts;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

public class CrudContractStatusService :
    CrudAppService<ContractStatus, ContractStatusDto, Guid, PagedAndSortedResultRequestDto,
        CreateUpdateContractStatusDto>,
    ICrudContractStatusService
{
    public CrudContractStatusService(IRepository<ContractStatus, Guid> repository) : base(repository)
    {
    }

    [Authorize(ContractStatusesPermissions.Statuses.Create)]
    public override Task<ContractStatusDto> CreateAsync(CreateUpdateContractStatusDto input) => base.CreateAsync(input);

    [Authorize(ContractStatusesPermissions.Statuses.Get)]
    public Task<ContractStatusDto> GetByCodeAsync(string code) => this.TryDisableMultiTenantAsync(async () =>
                                                                       {
                                                                           var status = await this.Repository.GetAsync(x => x.Code == code);
                                                                           return await base.MapToGetOutputDtoAsync(status);
                                                                       });

    [Authorize(ContractStatusesPermissions.Statuses.Get)]
    public override Task<ContractStatusDto> GetAsync(Guid id) => this.TryDisableMultiTenantAsync(() => base.GetAsync(id));

    [Authorize(ContractStatusesPermissions.Statuses.Get)]
    public override Task<PagedResultDto<ContractStatusDto>> GetListAsync(PagedAndSortedResultRequestDto input) => this.TryDisableMultiTenantAsync(() => base.GetListAsync(input));

    [Authorize(ContractStatusesPermissions.Statuses.Delete)]
    public override Task DeleteAsync(Guid id) => this.TryDisableMultiTenantAsync(() => base.DeleteAsync(id));

    [Authorize(ContractStatusesPermissions.Statuses.Edit)]
    public override Task<ContractStatusDto> UpdateAsync(Guid id, CreateUpdateContractStatusDto input) => this.TryDisableMultiTenantAsync(() => base.UpdateAsync(id, input));

    public Task<List<ContractStatusDto>> GetContractStatusesListAsync() => this.TryDisableMultiTenantAsync(async () =>
                                                                                {
                                                                                    var contractsStatuses = await this.Repository.ToListAsync();
                                                                                    var dtos = this.ObjectMapper.Map<List<ContractStatus>, List<ContractStatusDto>>(contractsStatuses);
                                                                                    return dtos;
                                                                                });

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
    [Authorize(ContractStatusesPermissions.Statuses.Get)]
    public async Task<List<ContractStatusDto>> GetContractStatusesByTenantIdAsync(Guid tenantId)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var contractsStatuses = (await this.Repository.WithDetailsAsync()).Where(x => x.TenantId == tenantId).OrderBy(x => x.Title).ToList();
            var dtos = this.ObjectMapper.Map<List<ContractStatus>, List<ContractStatusDto>>(contractsStatuses);
            return dtos;
        }


    }
    [Authorize(ContractStatusesPermissions.Statuses.Get)]
    public async Task<ContractStatusDto> GetByIdAsync(Guid id)
    {
        var status = (await Repository.FindAsync(x => x.Id == id));
        var dto = ObjectMapper.Map<ContractStatus, ContractStatusDto>(status);
        return dto;
    }
}
