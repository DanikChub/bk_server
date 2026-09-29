namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface IConstraintAppService : IApplicationService
{
    Task<List<ContractSettingDto>> GetConstraintTypeBySettingContractIdAsync(Guid contractId);
    Task<List<ConstraintTypeDto>> GetConstraintTypesAsync();
}
