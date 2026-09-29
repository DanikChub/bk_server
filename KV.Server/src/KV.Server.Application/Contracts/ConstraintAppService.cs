namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.Permissions.Contracts;
using KV.Server.TicketHoursSpent;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

[Authorize(ContractPermissions.Contracts.Default)]
public class ConstraintAppService : ApplicationService, IConstraintAppService
{
    private readonly IRepository<ConstraintType, Guid> _constraintTypeRepository;
    private readonly IRepository<ContractSetting, Guid> _contractSettingRepository;

    public ConstraintAppService(IRepository<ConstraintType, Guid> constraintTypeRepository,
        IRepository<ContractSetting, Guid> contractSettingRepository)
    {
        this._constraintTypeRepository = constraintTypeRepository;
        this._contractSettingRepository = contractSettingRepository;
    }

    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<List<ContractSettingDto>> GetConstraintTypeBySettingContractIdAsync(Guid contractId)
    {
        var constraintTypes = await this._constraintTypeRepository.ToListAsync();

        var query = await this._contractSettingRepository.WithDetailsAsync(x => x.ConstraintType);
        query = query.Where(x => x.ContractId == contractId);
        var nowConstraintTypes = query.ToList();
        foreach (var constraintType in constraintTypes)
        {
            var nowConstraintType = nowConstraintTypes.FirstOrDefault(x => x.ConstraintTypeId == constraintType.Id);
            if (nowConstraintType == null)
            {
                nowConstraintTypes.Add(new ContractSetting(0, constraintType.Id, contractId));
            }
        }

        var dtos = this.ObjectMapper.Map<List<ContractSetting>, List<ContractSettingDto>>(nowConstraintTypes);
        return dtos;
    }

    public async Task<List<ConstraintTypeDto>> GetConstraintTypesAsync()
    {
        var constraintTypes = await this._constraintTypeRepository.ToListAsync();
        var dtos = this.ObjectMapper.Map<List<ConstraintType>, List<ConstraintTypeDto>>(constraintTypes);
        return dtos;
    }
}
