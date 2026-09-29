namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface ICrudServicePackageService : ICrudAppService<ServicePackageDto, Guid, GetServicePackageListRequestDto,
    CreateUpdateServicePackageDto>
{
    Task DeleteServicePackageByIdAsync(Guid contractId, Guid packageId);
    Task<List<ServicePackageDto>> ToListAsync();
    Task<List<ServicePackageDto>> GetListServicePackageByTenantIdAsync(Guid tenantId);
    Task<List<ServicePackageDto>> GetListServicePacakageAsync();
}
