namespace KV.Server;
using System;
using System.Threading.Tasks;
using KV.Server.Localization;
using KV.Server.Permissions.Contracts;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using static Volo.Abp.TenantManagement.TenantManagementPermissions;

/// <summary>
///     При создании тенанта создает сущность TenantProfile.
/// </summary>
public class TenantCreatedDistributedEventHandler : IDistributedEventHandler<EntityCreatedEto<TenantEto>>,
    ITransientDependency
{
    private readonly IRepository<ContractStatus, Guid> _contractStatusRepository;
    private readonly IStringLocalizer<ServerResource> _localizer;
    private readonly ILogger<TenantCreatedDistributedEventHandler> _logger;
    private readonly IRepository<TenantProfile, Guid> _repositoty;
    private readonly IIdentityRoleAppService _identityRoleAppService;
    private readonly ICurrentTenant _currentTenant;
    private readonly IPermissionManager _permissionManager;
    public TenantCreatedDistributedEventHandler(IRepository<TenantProfile, Guid> repository,
        IRepository<ContractStatus, Guid> contractStatusRepository,
        IStringLocalizer<ServerResource> localizer,
        ILogger<TenantCreatedDistributedEventHandler> logger,
        IIdentityRoleAppService identityRoleAppService,
        ICurrentTenant currentTenant,
        IPermissionManager permissionManager)
    {
        this._repositoty = repository;
        this._contractStatusRepository = contractStatusRepository;
        this._localizer = localizer;
        this._logger = logger;
        _identityRoleAppService = identityRoleAppService;
        _currentTenant = currentTenant;
        _permissionManager = permissionManager;
    }

    public async Task HandleEventAsync(EntityCreatedEto<TenantEto> eventData)
    {
        try
        {
            using (_currentTenant.Change(eventData.Entity.Id))
            {

                var t = new TenantProfile(eventData.Entity.Id, eventData.Entity.Name, string.Empty, string.Empty);
                var tenantProfile = await this._repositoty.InsertAsync(t, true);
            }

        }
        catch (Exception exc)
        {
            this._logger.LogException(exc);
        }
    }


}
