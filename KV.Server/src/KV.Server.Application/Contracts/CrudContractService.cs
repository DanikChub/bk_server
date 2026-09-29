namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.Permissions.Contracts;
using KV.Server.Tags;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

[Authorize(ContractPermissions.Contracts.Default)]
public class CrudContractService :
    CrudAppService<Contract, ContractDto, Guid, GetContractListRequestDto, CreateUpdateContractDto>,
    ICrudContractService
{
    private readonly IRepository<ContractServicePackage> _contractServicePackageRepository;
    private readonly IRepository<TenantProfileManager> _tenantProfileManagerRepository;
    private readonly IRepository<TenantProfile, Guid> _tenantProfileRepository;
    private readonly IRepository<ContractSetting, Guid> _contractSettingRepository;
    private readonly IRepository<ContractStatus, Guid> _contractStatusRepository;
    private readonly IRepository<ContractTag> _contractTagRepository;
    private readonly IRepository<Tag, Guid> _tagRepository;

    public CrudContractService(IRepository<Contract, Guid> repository,
        IRepository<TenantProfile, Guid> tenantProfileRepository,
        IRepository<TenantProfileManager> tenantProfileManagerRepository,
        IRepository<ContractServicePackage> contractServicePackageRepository,
        IRepository<ContractSetting, Guid> contractSettingRepository,
        IRepository<ContractStatus, Guid> contractStatusRepository,
        IRepository<ContractTag> contractTagRepository,
        IRepository<Tag, Guid> tagRepository) : base(repository)
    {
        this._tenantProfileRepository = tenantProfileRepository;
        this._tenantProfileManagerRepository = tenantProfileManagerRepository;
        this._contractServicePackageRepository = contractServicePackageRepository;
        this._contractSettingRepository = contractSettingRepository;
        this._contractStatusRepository = contractStatusRepository;
        _contractTagRepository = contractTagRepository;
        _tagRepository = tagRepository;
    }

    public const string CompleteStatusLowerCaseName = "complete";
    public const string IsActiveLowerCaseName = "active";

    [Authorize(ContractPermissions.Contracts.Get)]
    public override async Task<PagedResultDto<ContractDto>> GetListAsync(GetContractListRequestDto input) => await this.TryDisableMultiTenantAsync(async () =>
                                                                                                                    {
                                                                                                                        var query = (await this.Repository.WithDetailsAsync(x => x.Status, x => x.TenantProfile,
                                                                                                                            x => x.ServicePackages))
                                                                                                                            .AsQueryable().Where(x=> !x.TenantProfile.IsDeleted );

                                                                                                                        if (input.TenantId is not null)
                                                                                                                        {
                                                                                                                            query = query.Where(x => x.TenantId == input.TenantId);
                                                                                                                        }

                                                                                                                        if (!string.IsNullOrWhiteSpace(input.ManagerId))
                                                                                                                        {
                                                                                                                            var tenantProfiles = ((await this._tenantProfileRepository.GetQueryableAsync())
                                                                                                                                    .Where(x => x.ResponsibleManagerId == Guid.Parse(input.ManagerId))
                                                                                                                                    .Select(x => x.Id)
                                                                                                                                    .ToList())
                                                                                                                                .Cast<Guid?>()
                                                                                                                                .ToList();

                                                                                                                            var tenantProfileManagers = ((await this._tenantProfileManagerRepository.GetQueryableAsync())
                                                                                                                                    .Where(x => x.ManagerId == Guid.Parse(input.ManagerId))
                                                                                                                                    .Select(x => x.TenantProfileId)
                                                                                                                                    .ToList())
                                                                                                                                .Cast<Guid?>()
                                                                                                                                .ToList();

                                                                                                                            query = query.Where(x => tenantProfiles.Contains(x.TenantId)
                                                                                                                                                        || tenantProfileManagers.Contains(x.TenantId));
                                                                                                                        }

                                                                                                                        if (!string.IsNullOrWhiteSpace(input.SearchByActive))
                                                                                                                        {
                                                                                                                            var completeStatus = await this._contractStatusRepository.FirstOrDefaultAsync(
                                                                                                                                x => x.Code.ToLower() == CompleteStatusLowerCaseName);
                                                                                                                            if (completeStatus != null)
                                                                                                                            {
                                                                                                                                if (input.SearchByActive == IsActiveLowerCaseName)
                                                                                                                                {
                                                                                                                                    query = query.Where(x => x.StatusId == completeStatus.Id);
                                                                                                                                }
                                                                                                                                else
                                                                                                                                {
                                                                                                                                    query = query.Where(x => x.StatusId != completeStatus.Id);
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }

                                                                                                                        if (!string.IsNullOrWhiteSpace(input.SearchName))
                                                                                                                        {
                                                                                                                            query = query.Where(x => x.Name.ToLower().Contains(input.SearchName.ToLower().Trim()));
                                                                                                                        }

                                                                                                                        if (!string.IsNullOrWhiteSpace(input.SearchContractStartDate) && DateTime.TryParse(input.SearchContractStartDate, out var searchContractStartDate))
                                                                                                                        {
                                                                                                                            query = query.Where(x => x.ContractStartDate >= searchContractStartDate);
                                                                                                                        }

                                                                                                                        if (!string.IsNullOrWhiteSpace(input.SearchContractFinishDate) && DateTime.TryParse(input.SearchContractFinishDate, out var searchContractFinishDate))
                                                                                                                        {
                                                                                                                            query = query.Where(x => x.ContractFinishDate <= searchContractFinishDate);
                                                                                                                        }

                                                                                                                        if (!string.IsNullOrWhiteSpace(input.SearchStatusId))
                                                                                                                        {
                                                                                                                            query = query.Where(x => x.StatusId == Guid.Parse(input.SearchStatusId));
                                                                                                                        }

                                                                                                                        if (!string.IsNullOrWhiteSpace(input.SearchServicePackageId))
                                                                                                                        {
                                                                                                                            query = query.Where(x =>
                                                                                                                                x.ServicePackages.Any(x => x.ServicePackageId == Guid.Parse(input.SearchServicePackageId)));
                                                                                                                        }

                                                                                                                        if (!string.IsNullOrWhiteSpace(input.SearchTenantProfileShortName))
                                                                                                                        {
                                                                                                                            query = query.Where(x =>
                                                                                                                                x.TenantProfile.DisplayName.ToLower()
                                                                                                                                    .Contains(input.SearchTenantProfileShortName.ToLower().Trim()) ||
                                                                                                                                x.TenantProfile.ShortName.ToLower()
                                                                                                                                    .Contains(input.SearchTenantProfileShortName.ToLower().Trim()));
                                                                                                                        }

                                                                                                                        if (!string.IsNullOrEmpty(input.Sorting))
                                                                                                                        {
                                                                                                                            if (input.Sorting == "tenantProfileShortName asc")
                                                                                                                            {
                                                                                                                                query = query.OrderBy(x => x.TenantProfile.ShortName);
                                                                                                                            }
                                                                                                                            else if (input.Sorting == "statusTitle asc")
                                                                                                                            {
                                                                                                                                query = query.OrderBy(x => x.Status.Title);
                                                                                                                            }
                                                                                                                            else if (input.Sorting == "servicePackageName asc")
                                                                                                                            {
                                                                                                                                query = query.OrderBy(x => x.ServicePackages.FirstOrDefault().ServicePackage.Name);
                                                                                                                            }
                                                                                                                            else if (input.Sorting == "tenantProfileShortName desc")
                                                                                                                            {
                                                                                                                                query = query.OrderByDescending(x => x.Status.Title);
                                                                                                                            }
                                                                                                                            else if (input.Sorting == "statusTitle desc")
                                                                                                                            {
                                                                                                                                query = query.OrderByDescending(x => x.Status.Title);
                                                                                                                            }
                                                                                                                            else if (input.Sorting == "servicePackageName desc")
                                                                                                                            {
                                                                                                                                query = query.OrderByDescending(x => x.ServicePackages.FirstOrDefault().ServicePackage.Name);
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                query = query.OrderBy(input.Sorting);
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else
                                                                                                                        {
                                                                                                                            query = query.OrderByDescending(x => x.CreationTime);
                                                                                                                        }

                                                                                                                        var totalCount = query.Count();
                                                                                                                        var contracts = query
                                                                                                                            .Skip(input.SkipCount)
                                                                                                                            .Take(input.MaxResultCount == 0 ? GetContractListRequestDto.DefaultPageSize : input.MaxResultCount)
                                                                                                                            .ToList();
                                                                                                                        var contractDtos = this.ObjectMapper.Map<List<Contract>, List<ContractDto>>(contracts);

                                                                                                                        return new PagedResultDto<ContractDto>(totalCount, contractDtos);
                                                                                                                    });

    [Authorize(ContractPermissions.Contracts.Get)]
    public override Task<ContractDto> GetAsync(Guid id) => this.TryDisableMultiTenantAsync(async () =>
        {
            var details = (await this.Repository.WithDetailsAsync(x => x.Status, x => x.TenantProfile))
                .FirstOrDefault(x => x.Id == id);
            return await this.MapToGetOutputDtoAsync(details);
        });

    [Authorize(ContractPermissions.Contracts.Edit)]
    public override Task<ContractDto> UpdateAsync(Guid id, CreateUpdateContractDto input) => this.TryDisableMultiTenantAsync(() => base.UpdateAsync(id, input));

    [Authorize(ContractPermissions.Contracts.Edit)]
    public async Task UpdateDetailsAsync(Guid id, CreateUpdateDetailsContractDto input)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var contractServicePackage =
                await this._contractServicePackageRepository.FirstOrDefaultAsync(x => x.ContractId == id);
            var contract = await this.Repository.FirstOrDefaultAsync(x => x.Id == id);
            if (input.ServicePackageId != null && contract != null)
            {
                if (contractServicePackage != null)
                {
                    await this._contractServicePackageRepository.DeleteAsync(contractServicePackage);
                    await this.CurrentUnitOfWork.SaveChangesAsync();
                }

                var newContractServicePackage = new ContractServicePackage
                {
                    ContractId = id,
                    ServicePackageId = input.ServicePackageId ?? Guid.Empty,
                    ContractFinishDate = contract.ContractFinishDate,
                    ContractStartDate = contract.ContractStartDate
                };
                await this._contractServicePackageRepository.InsertAsync(newContractServicePackage);
                await this.CurrentUnitOfWork.SaveChangesAsync();
            }
        }
    }

    [Authorize(ContractPermissions.Contracts.Edit)]
    public async Task UpdateMaxConstraintAsync(Guid id, Guid constraintTypeId, int max)
    {
        var contractSetting = await this._contractSettingRepository.FirstOrDefaultAsync(x => x.ContractId == id &&
            x.ConstraintTypeId == constraintTypeId);
        if (contractSetting == null)
        {
            await this._contractSettingRepository.InsertAsync(new ContractSetting(max, constraintTypeId, id));
        }
        else
        {
            contractSetting.Max = max;
            await this._contractSettingRepository.UpdateAsync(contractSetting, true);
        }
    }

    [Authorize(ContractPermissions.Contracts.Delete)]
    public override Task DeleteAsync(Guid id) => this.TryDisableMultiTenantAsync(() => base.DeleteAsync(id));

    [Authorize(ContractPermissions.Contracts.Create)]
    public override async Task<ContractDto> CreateAsync(CreateUpdateContractDto input)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var contract = new Contract();
            var status = (await _contractStatusRepository.WithDetailsAsync()).Where(x => x.Code == input.StatusCode && x.TenantId == input.TenantId).FirstOrDefault();
            contract.StatusId = status.Id;
            contract.ContractStartDate = input.ContractStartDate;
            contract.ContractFinishDate = input.ContractFinishDate;
            contract.TenantId = input.TenantId;
            contract.Name = input.Name;
            await Repository.InsertAsync(contract);
            var dto = ObjectMapper.Map<Contract, ContractDto>(contract);
            return dto;
        }
    }

    protected override async Task<IQueryable<Contract>> CreateFilteredQueryAsync(GetContractListRequestDto input) => await this.Repository.WithDetailsAsync(x => x.Status, x => x.TenantProfile);

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
    [Authorize(ContractPermissions.Contracts.Edit)]
    public async Task CreateTagByContractIdAsync(Guid contractid, string tag)
    {
        var createdTag = new Tag
        {
            Name = tag
        };
        var tagExists = (await _tagRepository.WithDetailsAsync()).Where(x => x.Name.Contains(tag)).FirstOrDefault();
        if (tagExists != null)
        {
            var contractExistsTag = new ContractTag
            {
                ContractId = contractid,
                TagId = tagExists.Id
            };
            await _contractTagRepository.InsertAsync(contractExistsTag);
            return;
        }
        var newTag = await _tagRepository.InsertAsync(createdTag);
        var contractTag = new ContractTag
        {
            ContractId = contractid,
            TagId = newTag.Id
        };
        await _contractTagRepository.InsertAsync(contractTag);

    }

    public async Task<List<TagDto>> GetTagListByContractIdAsync(Guid contractid)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var tags = (await _contractTagRepository.WithDetailsAsync(x => x.Tag)).Where(x => x.ContractId == contractid).Select(x => x.Tag).ToList();
            var dtos = ObjectMapper.Map<List<Tag>, List<TagDto>>(tags);

            return dtos;
        }

    }

    public async Task DeleteTagByIdAsync(Guid tagId)
    {
        using (this.DataFilter.Disable<IMultiTenant>())
        {
            var tag = (await _contractTagRepository.WithDetailsAsync(x => x.Tag)).FirstOrDefault(x => x.TagId == tagId);
            await _contractTagRepository.DeleteAsync(tag);
        }
    }
}
