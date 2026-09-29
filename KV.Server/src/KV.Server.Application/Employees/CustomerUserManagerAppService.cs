namespace KV.Server.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Permissions;
using KV.Server.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

public class CustomerUserManagerAppService : ApplicationService, ICustomerUserManagerAppService
{
    private readonly IRepository<TenantProfileManager> _tenantProfileManagerRepository;
    private readonly IRepository<TenantProfile, Guid> _tenantProfileRepository;

    public CustomerUserManagerAppService(IRepository<TenantProfile, Guid> tenantProfileRepository,
        IRepository<TenantProfileManager> tenantProfileManagerRepository)
    {
        this._tenantProfileRepository = tenantProfileRepository;
        this._tenantProfileManagerRepository = tenantProfileManagerRepository;
    }

    [Authorize(ManagerPermissions.Profiles.Get)]
    public async Task<PagedResultDto<CustomerUserProfileDto>> GetListByTenantIdAsync(
        GetCustomerManagerListRequestDto input)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var query = await this._tenantProfileManagerRepository.WithDetailsAsync(x => x.Manager);

            query = query.Where(x => x.TenantProfileId == input.CustomerId);
            var totalCount = query.Count();
            var managers = query
                .Select(x => x.Manager)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount == 0
                    ? GetCustomerManagerListRequestDto.DefaultPageSize
                    : input.MaxResultCount)
                .ToList();
            var dtos = this.ObjectMapper.Map<List<CustomerUserProfile>, List<CustomerUserProfileDto>>(managers);
            return new PagedResultDto<CustomerUserProfileDto>(totalCount, dtos);
        }
    }

    [Authorize(ManagerPermissions.Profiles.Create)]
    public async Task AddManagerByTenantIdAsync(Guid customerUserProfileId, Guid tenantId)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            await this._tenantProfileManagerRepository.InsertAsync(new TenantProfileManager
            {
                ManagerId = customerUserProfileId,
                TenantProfileId = tenantId
            });
        }
    }

    [Authorize(ManagerPermissions.Profiles.Delete)]
    public async Task DeleteManagerByTenantIdAsync(Guid customerUserProfileId, Guid tenantId)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var tenantProfileManager = await this._tenantProfileManagerRepository.FirstOrDefaultAsync(
                x => x.TenantProfileId == tenantId && x.ManagerId == customerUserProfileId);
            if (tenantProfileManager != null)
            {
                await this._tenantProfileManagerRepository.DeleteAsync(tenantProfileManager);
            }
        }
    }
}
