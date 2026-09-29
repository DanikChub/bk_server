namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.Permissions.Contracts;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

public class CrudServicePackageService :
    CrudAppService<ServicePackage, ServicePackageDto, Guid, GetServicePackageListRequestDto,
        CreateUpdateServicePackageDto>,
    ICrudServicePackageService
{
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IRepository<ContractServicePackage> _contractServicePackageRepository;

    public CrudServicePackageService(IRepository<ServicePackage, Guid> repository,
        IRepository<Contract, Guid> contractRepository,
        IRepository<ContractServicePackage> contractServicePackageRepository) : base(repository)
    {
        this._contractRepository = contractRepository;
        this._contractServicePackageRepository = contractServicePackageRepository;
    }

    [Authorize(ContractStatusesPermissions.Statuses.Create)]
    public override Task<ServicePackageDto> CreateAsync(CreateUpdateServicePackageDto input) => base.CreateAsync(input);

    [Authorize(ContractStatusesPermissions.Statuses.Get)]
    public override Task<ServicePackageDto> GetAsync(Guid id) => this.TryDisableMultiTenantAsync(() => base.GetAsync(id));

    [Authorize(ContractStatusesPermissions.Statuses.Get)]
    public override Task<PagedResultDto<ServicePackageDto>> GetListAsync(GetServicePackageListRequestDto input) => this.TryDisableMultiTenantAsync(() => base.GetListAsync(input));

    [Authorize(ContractStatusesPermissions.Statuses.Delete)]
    public override Task DeleteAsync(Guid id) => this.TryDisableMultiTenantAsync(() => base.DeleteAsync(id));

    [Authorize(ContractStatusesPermissions.Statuses.Edit)]
    public override Task<ServicePackageDto> UpdateAsync(Guid id, CreateUpdateServicePackageDto input) => this.TryDisableMultiTenantAsync(() => base.UpdateAsync(id, input));
    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<List<ServicePackageDto>> ToListAsync()
    {
        if (this.CurrentTenant.Id is null)
        {
            using (this.DataFilter.Disable<IMultiTenant>())
            {
                var servicePackages = await this.Repository.ToListAsync();
                return this.ObjectMapper.Map<List<ServicePackage>, List<ServicePackageDto>>(servicePackages);
            }
        }

        var servicePackagesWithoutTenant = await this.Repository.ToListAsync();
        return this.ObjectMapper.Map<List<ServicePackage>, List<ServicePackageDto>>(servicePackagesWithoutTenant);
    }
    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<List<ServicePackageDto>> GetListServicePacakageAsync()
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var servicePackages = await this.Repository.ToListAsync();
            var dto = this.ObjectMapper.Map<List<ServicePackage>, List<ServicePackageDto>>(servicePackages);
            return dto;
        }
    }
    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<List<ServicePackageDto>> GetListServicePackageByTenantIdAsync(Guid tenantId)
    {
        if (this.CurrentTenant.Id is null)
        {
            using (this.DataFilter.Disable<IMultiTenant>())
            {
                var servicePackages = await this.Repository.ToListAsync();
                return this.ObjectMapper.Map<List<ServicePackage>, List<ServicePackageDto>>(servicePackages);
            }
        }

        var contract = (await this._contractRepository
            .WithDetailsAsync(x => x.ServicePackages))
            .FirstOrDefault(x => x.TenantId == tenantId);
        var servicePackagesWithTenantId = contract.ServicePackages
            .Select(x => x.ServicePackage)
            .ToList();
        return this.ObjectMapper.Map<List<ServicePackage>, List<ServicePackageDto>>(servicePackagesWithTenantId);
    }

    private async Task<T> TryDisableMultiTenantAsync<T>(Func<Task<T>> func)
    {
        if (this.CurrentTenant.Id is null)
        {
            using (this.DataFilter.Disable<IMultiTenant>())
            {
                return await func();
            }
        }

        return await func();
    }

    private async Task TryDisableMultiTenantAsync(Func<Task> func)
    {
        if (this.CurrentTenant.Id is null)
        {
            using (this.DataFilter.Disable<IMultiTenant>())
            {
                await func();
            }
        }

        await func();
    }
    [Authorize(ContractPermissions.Contracts.Delete)]
    public async Task DeleteServicePackageByIdAsync(Guid contractId, Guid packageId)
    {
        var servicePackage = (await this.Repository.WithDetailsAsync(x => x.ServicePackegs)).Where(x => x.Id == packageId).FirstOrDefault();
        if (servicePackage != null)
        {
            var contractServicePackage = (await this._contractServicePackageRepository.WithDetailsAsync()).Where(x => x.ServicePackageId == servicePackage.Id && x.ContractId == contractId).FirstOrDefault();
            if (contractServicePackage != null)
            {
                await this._contractServicePackageRepository.DeleteAsync(contractServicePackage);
            }
        }
    }
}
