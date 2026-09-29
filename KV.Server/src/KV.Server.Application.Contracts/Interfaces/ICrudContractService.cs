namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface
    ICrudContractService : ICrudAppService<ContractDto, Guid, GetContractListRequestDto, CreateUpdateContractDto>
{
    Task UpdateDetailsAsync(Guid id, CreateUpdateDetailsContractDto input);
    Task CreateTagByContractIdAsync(Guid contractid, string tag);
    Task<List<TagDto>> GetTagListByContractIdAsync(Guid contractid);
    Task DeleteTagByIdAsync(Guid tagId);
    Task UpdateMaxConstraintAsync(Guid id, Guid constraintTypeId, int max);
}
