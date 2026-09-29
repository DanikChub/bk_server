using System.Threading.Tasks;
using KV.Server.Dtos.Region;
using Volo.Abp.Application.Dtos;

namespace KV.Server.Interfaces;
public interface IRegionsAppService
{
    Task<ListResultDto<RegionDto>> GetListAsync();
}
