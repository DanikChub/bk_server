namespace KV.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface IContractsAppService : IApplicationService
{
    Task<List<ContractDto>> GetListContractByTenantIdAsync(Guid tenantId);

    Task<PagedResultDto<ContractDto>> GetContractListByTenantIdAsync(Guid tenantId, GetContractListRequestDto dto,
        int skip, int take, string name);

    Task<int> GetCountContractsByTenantIdAsync(Guid tenantId);
    Task<ContractDto> GetFirstActiveContractAsync(Guid? tenantId);
    Task<ContractDto> GetContractAsync(Guid contractId);
    Task<List<ServicePackageDto>> GetListActiveServicePackagesByTenantIdAsync(Guid? tenantId);
    Task<List<ContractDto>> GetListActiveContractByTenantIdAsync(Guid? tenantId);

    Task<List<ServicePackageDto>> GetListNotActiveServicePackagesByTenantIdAsync(Guid? tenantId);
    Task<ContractDto> GetContractByTicketIdAsync(long ticketId);
    Task<List<ServicePackageDto>> GetServicePackagesByContractIdAsync(Guid contractId);
    Task<List<ServicePackageDto>> GetListServicePackagesAsync();
    Task CreateContractServicePackageAsync(Guid contractId, Guid serviceId, DateTime contractStartDate, DateTime contractFinishDate);
}
