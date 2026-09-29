using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Web.BackgroundJobs.Args;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace KV.Server.Web.BackgroundJobs;

public class UpdateCustomerStatusJob : AsyncBackgroundJob<UpdateCustomerStatusArgs>, ITransientDependency
{
    private readonly ILogger<UpdateCustomerStatusJob> _logger;
    private readonly IDataFilter _dataFilter;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<TenantProfile> _tenantProfileRepository;
    private readonly IRepository<Contract> _contractRepository;
    private readonly IObjectMapper _objectMapper;

    public UpdateCustomerStatusJob(
        ILogger<UpdateCustomerStatusJob> logger,
        IDataFilter dataFilter,
        IUnitOfWorkManager unitOfWorkManager,
        ICurrentTenant currentTenant,
        IRepository<TenantProfile, Guid> tenantProfileRepository,
        IRepository<Contract, Guid> contractRepository,
        IObjectMapper objectMapper
        )
    {
        _logger = logger;
        _dataFilter = dataFilter;
        _unitOfWorkManager = unitOfWorkManager;
        _currentTenant = currentTenant;
        _tenantProfileRepository = tenantProfileRepository;
        _contractRepository = contractRepository;
        _objectMapper = objectMapper;
    }

    public override async Task ExecuteAsync(UpdateCustomerStatusArgs args)
    {
        try
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                using (_dataFilter.Disable<IMultiTenant>())
                {
                    var totalCount = await GetCustomersCountAsync();

                    for (var skip = 0; skip < totalCount; skip += 100)
                    {
                        var customers = await GetCustomersAsync(new GetCustomerListRequestDto { Sorting = "Id", MaxResultCount = 100, SkipCount = skip });

                        foreach (var customer in customers.Items)
                        {
                            var contractsCount = await GetActiveContractsCountAsync(customer.Id);

                            await UpdateCustomerActiveStatusAsync(customer.Id, contractsCount > 0);

                            await uow.SaveChangesAsync();
                        }
                    }
                }

                await uow.CompleteAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update customers statuses");
        }
    }

    private async Task<PagedResultDto<CustomerDto>> GetCustomersAsync(GetCustomerListRequestDto input)
    {
        var queryable = (await _tenantProfileRepository.GetQueryableAsync())
            .Take(input.MaxResultCount)
            .Skip(input.SkipCount)
            .OrderBy(x => x.Id);

        var entities = await queryable.ToListAsync();

        var dtos = _objectMapper.Map<List<TenantProfile>, List<CustomerDto>>(entities);
        var totalCount = await queryable.CountAsync();

        return new PagedResultDto<CustomerDto> { Items = dtos, TotalCount = totalCount };
    }

    private async Task<int> GetCustomersCountAsync()
    {
        var queryable = (await _tenantProfileRepository.GetQueryableAsync())
            .OrderBy(x => x.Id);

        return await queryable.CountAsync();
    }

    [UnitOfWork]
    private async Task UpdateCustomerActiveStatusAsync(Guid id, bool isActive)
    {
            var customer = await _tenantProfileRepository.FirstOrDefaultAsync(x => x.Id == id);

            if (customer.IsActive != isActive)
                customer.IsActive = isActive;
            await _tenantProfileRepository.UpdateAsync(customer);
    }

    private async Task<int> GetActiveContractsCountAsync(Guid tid)
    {
        var queryable = (await _contractRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tid && x.ContractFinishDate >= DateTime.Now)
            .OrderBy(x => x.Id);

        return await queryable.CountAsync();
    }
}
