namespace KV.Server;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface ICustomerUserManagerAppService : IApplicationService
{
    Task<PagedResultDto<CustomerUserProfileDto>> GetListByTenantIdAsync(GetCustomerManagerListRequestDto input);
    Task AddManagerByTenantIdAsync(Guid customerUserProfileId, Guid tenantId);
    Task DeleteManagerByTenantIdAsync(Guid customerUserProfileId, Guid tenantId);
}
