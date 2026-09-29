namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.Interfaces;
using KV.Server.Permissions.Contracts;
using KV.Server.Profiles;
using KV.Server.Tickets;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

public class ContractsAppService : ApplicationService, IContractsAppService
{
    private readonly IRepository<ContractServicePackage> _contractServicePackageRepostiry;
    private readonly IRepository<Contract, Guid> _contractsRepository;
    private readonly IRepository<CustomerUserProfile, Guid> _customersRepostiry;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<ServicePackage, Guid> _servicePackageRepostiry;
    private readonly IRepository<TenantProfile, Guid> _tenantProfileRepostiry;
    private readonly IRepository<Ticket, long> _ticketRepository;

    public ContractsAppService(IRepository<Contract, Guid> contractsRepostiry,
        IRepository<CustomerUserProfile, Guid> customersRepostiry,
        IRepository<TenantProfile, Guid> tenantProfileRepostiry,
        IRepository<ServicePackage, Guid> servicePackageRepostiry,
        IRepository<ContractServicePackage> contractServicePackageRepostiry,
        IRepository<Ticket, long> ticketRepository,
        IDataFilter dataFilter)
    {
        this._contractsRepository = contractsRepostiry;
        this._customersRepostiry = customersRepostiry;
        this._tenantProfileRepostiry = tenantProfileRepostiry;
        this._servicePackageRepostiry = servicePackageRepostiry;
        this._contractServicePackageRepostiry = contractServicePackageRepostiry;
        this._ticketRepository = ticketRepository;
        this._dataFilter = dataFilter;
    }

    public async Task<PagedResultDto<ContractDto>> GetContractListByTenantIdAsync(Guid tenantId,
        GetContractListRequestDto dto, int skip, int take, string name)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._contractsRepository.WithDetailsAsync(x => x.Status, x => x.TenantProfile, x => x.ServicePackages);
            query = query.Where(x => x.TenantProfile.Id == tenantId);
            if (!string.IsNullOrWhiteSpace(dto.SearchName))
            {
                query = query.Where(x => x.Name.ToLowerInvariant().Contains(dto.SearchName.Trim().ToLowerInvariant()));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => x.Name.ToLowerInvariant().Contains(name.Trim().ToLowerInvariant()));
            }

            if (!string.IsNullOrEmpty(dto.Sorting))
            {
                query = query.OrderBy(dto.Sorting);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }

            var defaultTake = dto.MaxResultCount == 0
                ? GetContractListRequestDto.DefaultPageSize
                : dto.MaxResultCount;
            var totalCount = query.Count();
            var contracts = query
                .Skip(skip)
                .Take(take == 0 ? defaultTake : take)
                .ToList();
            var contractDtos = this.ObjectMapper.Map<List<Contract>, List<ContractDto>>(contracts);
            foreach (var contractDto in contractDtos)
            {
                contractDto.ServicePackages = await this.GetServicePackagesByContractIdAsync(contractDto.Id);
            }

            return new PagedResultDto<ContractDto>(totalCount, contractDtos);
        }
    }

    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<ContractDto> GetContractAsync(Guid contractId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var contract = (await this._contractsRepository
                    .WithDetailsAsync(x => x.Status, x => x.TenantProfile))
                .FirstOrDefault(x => x.Id == contractId);
            var contractDto = this.ObjectMapper.Map<Contract, ContractDto>(contract);
            contractDto.ServicePackages = await this.GetServicePackagesByContractIdAsync(contractId);
            return contractDto;
        }
    }

    public async Task<List<ServicePackageDto>> GetServicePackagesByContractIdAsync(Guid contractId)
    {
        var servicePackages = (await this._contractServicePackageRepostiry
                .WithDetailsAsync(x => x.ServicePackage))
            .Where(x => x.ContractId == contractId)
            .Select(x => x.ServicePackage)
            .ToList();
        var servicePackageDtos = this.ObjectMapper.Map<List<ServicePackage>, List<ServicePackageDto>>(servicePackages);
        return servicePackageDtos;
    }

    public async Task<int> GetCountContractsByTenantIdAsync(Guid tenantId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._contractsRepository.WithDetailsAsync(x => x.Status,
                x => x.ServicePackages); // сущность навигационое свойства
            query = query.Where(x => x.TenantId == tenantId);
            var count = query.Count();
            return count;
        }
    }

    public async Task<List<ContractDto>> GetListContractByTenantIdAsync(Guid tenantId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._contractsRepository.WithDetailsAsync(x => x.ServicePackages);
            var contracts = query
                .Where(x => x.TenantId == tenantId)
                .ToList();
            var dtos = this.ObjectMapper.Map<List<Contract>, List<ContractDto>>(contracts);
            foreach (var dto in dtos)
            {
                dto.ServicePackages = await this.GetServicePackagesByContractIdAsync(dto.Id);
            }

            return dtos;
        }
    }

    public async Task<List<ServicePackageDto>> GetListActiveServicePackagesByTenantIdAsync(Guid? tenantId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var contractServicePakages = (await this._contractServicePackageRepostiry
                    .WithDetailsAsync(x => x.Contract, x => x.ServicePackage))
                .Where(x => x.Contract.TenantId == tenantId)
                .ToList();

            var servicePackages = new List<ServicePackage>();
            foreach (var contractServicePakage in contractServicePakages)
            {
                var servicePackagesNew = contractServicePakage.ServicePackage;
                servicePackages.Add(servicePackagesNew);
            }

            servicePackages = servicePackages
                .GroupBy(car => car.Name)
                .Select(g => g.First())
                .ToList();
            var dtos = this.ObjectMapper.Map<List<ServicePackage>, List<ServicePackageDto>>(servicePackages);
            return dtos;
        }
    }

    public async Task<List<ServicePackageDto>> GetListNotActiveServicePackagesByTenantIdAsync(Guid? tenantId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var allServicePackages = await this._servicePackageRepostiry.ToListAsync();
            var activeServicePackages = await this.GetListActiveServicePackagesByTenantIdAsync(tenantId);
            var notActiveServicePackages = new List<ServicePackage>();
            foreach (var servicePackage in allServicePackages)
            {
                if (!activeServicePackages.Any(x => x.Id == servicePackage.Id))
                {
                    notActiveServicePackages.Add(servicePackage);
                }
            }

            var dtos = this.ObjectMapper.Map<List<ServicePackage>, List<ServicePackageDto>>(notActiveServicePackages);
            return dtos;
        }
    }

    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<ContractDto> GetContractByTicketIdAsync(long ticketId)
    {
        var ticket = (await this._ticketRepository
                .WithDetailsAsync(x => x.Contract))
            .FirstOrDefault(x => x.Id == ticketId);
        var contract = ticket?.Contract;
        if (contract == null)
        {
            var tenantId = ticket?.TenantId ?? Guid.Empty;
            var tenantProfile = await this._tenantProfileRepostiry
                .FirstOrDefaultAsync(x => x.Id == tenantId);
            if (tenantProfile != null)
            {
                contract = tenantProfile.Contracts
                    .OrderByDescending(x => x.ContractFinishDate)
                    .FirstOrDefault();
            }
        }

        var dto = this.ObjectMapper.Map<Contract, ContractDto>(contract);
        return dto;
    }
    [Authorize(ContractPermissions.Contracts.Create)]

    public async Task CreateContractServicePackageAsync(Guid contractId, Guid serviceId, DateTime contractStartDate, DateTime contractFinishDate)
    {
        var contractServicePackage = new ContractServicePackage
        {
            ServicePackageId = serviceId,
            ContractId = contractId,
            ContractStartDate = contractStartDate,
            ContractFinishDate = contractFinishDate
        };
        await this._contractServicePackageRepostiry.InsertAsync(contractServicePackage);

    }
    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<ContractDto> GetFirstActiveContractAsync(Guid? tenantId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var contractQuery = (await _contractsRepository.WithDetailsAsync(x => x.Status, x => x.TenantProfile));
            var contract = contractQuery.Where(x => x.TenantId == tenantId && (x.ContractStartDate <= Clock.Now && x.ContractFinishDate >= Clock.Now) && x.Status.Code == ContractStatusConstants.STATUS_CODE_PROGRESS)
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
    }
    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<List<ContractDto>> GetListActiveContractByTenantIdAsync(Guid? tenantId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var contracts = (await _contractsRepository.WithDetailsAsync(x => x.Status, x => x.ServicePackages))
            .Where(x => x.TenantId == tenantId && (x.ContractStartDate <= Clock.Now && x.ContractFinishDate >= Clock.Now && x.Status.Code == ContractStatusConstants.STATUS_CODE_PROGRESS))
            .OrderByDescending(x => x.ContractStartDate).ToList();
            var dtoList = ObjectMapper.Map<List<Contract>, List<ContractDto>>(contracts);

            return dtoList;
        }
    }
    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<List<ServicePackageDto>> GetListServicePackagesAsync()
    {
        var servicePackages = await this._servicePackageRepostiry
             .GetQueryableAsync();
        var servicePackageDtos = this.ObjectMapper.Map<List<ServicePackage>, List<ServicePackageDto>>(servicePackages.ToList());
        return servicePackageDtos;
    }
}
