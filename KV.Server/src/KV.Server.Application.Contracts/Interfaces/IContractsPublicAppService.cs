using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KV.Server.Interfaces;
public interface IContractsPublicAppService
{
    Task<ContractDto> GetFirstActiveContractAsync();
    Task<List<ContractDto>> GetListActiveContractByTenantIdAsync(Guid? tenantId);
}
