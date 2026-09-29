namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

/// <summary>
///     Интерфейс для встроенного CRUD сервиса.
/// </summary>
public interface ICrudCustomerAppService :
    ICrudAppService<
        CustomerDto,
        Guid,
        GetCustomerListRequestDto,
        CreateUpdateCustomerDto>
{
    Task<PagedResultDto<ContractDto>> GetContractsByCustomerIdAsync(GetCustomerContractListRequestDto input);
    Task UpdateResponsibleManagerByTenantIdAsync(Guid? customerUserProfileId, Guid tenantId);
    Task UpdateContactUserProfileAsync(Guid? customerUserProfileId, Guid tenantId);
    Task<int> GetCountResponsibleManagerByCustomerUserProfileIdAsync(Guid customerUserProfileId);
    Task<CustomerDto> FirstOrDefaultByTenantIdAsync(Guid? tenantId);
    Task<List<CustomerServicePackageDto>> GetActiveCustomerServicePackagesAsync(Guid tenantId);

    Task<List<CustomerServicePackageDto>> GetActiveServicePackagesByContractIdAsync(Guid contractId);
    Task<PagedResultDto<CustomerEventDto>> GetCustomerEventsAsync(GetCustomerEventsListRequestDto input);
    Task<List<CustomerStatisticsDto>> GetStatisticsAsync(GetCustomerStatisticsListRequestDto input);
    Task<List<CustomerContractStatisticsDto>> GetStatisticsConstraintByContractIdAsync(Guid contractId);
    Task<PagedResultDto<CustomerDto>> GetListCustomersBySearchAsync(GetCustomerListRequestDto input);

    Task CreateClientRoleAsync(Guid customerId);

}
