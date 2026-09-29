using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.Internal.Mappers;
using KV.Server.Interfaces;
using KV.Server.Permissions.Contracts;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace KV.Server.Contracts;
public class ContractsPublicAppService : ApplicationService, IContractsPublicAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Contract, Guid> _contractsRepository;

    public ContractsPublicAppService(IRepository<Contract, Guid> contractsRepository, IDataFilter dataFilter)
    {
        _contractsRepository = contractsRepository;
        _dataFilter = dataFilter;
    }

    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<ContractDto> GetFirstActiveContractAsync()
    {

        var contractQuery = (await _contractsRepository.WithDetailsAsync(x => x.Status, x => x.TenantProfile));
        var contract = contractQuery.Where(x => (x.ContractStartDate <= Clock.Now && x.ContractFinishDate >= Clock.Now) && x.Status.Code == ContractStatusConstants.STATUS_CODE_PROGRESS)
        .OrderByDescending(x => x.ContractStartDate).FirstOrDefault();
        var dto = ObjectMapper.Map<Contract, ContractDto>(contract);
        if (dto == null)
        {
            return null;
        }
        else
        {
            return dto;
        }

    }
    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<List<ContractDto>> GetListActiveContractByTenantIdAsync(Guid? tenantId)
    {

        var contracts = (await _contractsRepository.WithDetailsAsync(x => x.Status, x => x.ServicePackages))
        .Where(x => x.TenantId == tenantId && (x.ContractStartDate <= Clock.Now && x.ContractFinishDate >= Clock.Now && x.Status.Code == ContractStatusConstants.STATUS_CODE_PROGRESS))
        .OrderByDescending(x => x.ContractStartDate).ToList();
        var dtoList = ObjectMapper.Map<List<Contract>, List<ContractDto>>(contracts);

        return dtoList;

    }
}
