namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

public interface ICrudContractStatusService : ICrudAppService<ContractStatusDto, Guid, PagedAndSortedResultRequestDto,
    CreateUpdateContractStatusDto>
{
    Task<ContractStatusDto> GetByCodeAsync(string code);
    Task<ContractStatusDto> GetByIdAsync(Guid id);
    Task<List<ContractStatusDto>> GetContractStatusesListAsync();

    Task<List<ContractStatusDto>> GetContractStatusesByTenantIdAsync(Guid tenantId);
}
