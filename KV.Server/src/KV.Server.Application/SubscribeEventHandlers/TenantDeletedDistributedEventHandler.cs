namespace KV.Server;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.TenantManagement;

/// <summary>
///     При удалении тенанта удаляет сущность TenantProfile.
/// </summary>
public class TenantDeletedDistributedEventHandler : IDistributedEventHandler<EntityDeletedEto<TenantEto>>,
    ITransientDependency
{
    private readonly IRepository<TenantProfile, Guid> _repositoty;

    public TenantDeletedDistributedEventHandler(IRepository<TenantProfile, Guid> repository)
    {
        this._repositoty = repository;
    }

    public async Task HandleEventAsync(EntityDeletedEto<TenantEto> eventData)
    {
        var id = eventData.Entity.Id;
        await this._repositoty.DeleteAsync(id, true);
    }
}
