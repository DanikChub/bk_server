using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using KV.Server.Dtos.Region;
using KV.Server.Interfaces;
using KV.Server.Tenants;
using KV.Server.TicketHoursSpent;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace KV.Server.Regions;
public class RegionsAppService : ApplicationService, IRegionsAppService
{
    private readonly IRepository<Region, Guid> _repository;
    private readonly IObjectMapper _objectMapper;

    public RegionsAppService(IRepository<Region, Guid> repository, IObjectMapper objectMapper)
    {
        _repository = repository;
        _objectMapper = objectMapper;
    }

    public async Task<ListResultDto<RegionDto>> GetListAsync()
    {
        var regions = await _repository.GetListAsync();

        return new ListResultDto<RegionDto> { Items = regions.Select(x => _objectMapper.Map<Region, RegionDto>(x)).ToList() };
    }
}
