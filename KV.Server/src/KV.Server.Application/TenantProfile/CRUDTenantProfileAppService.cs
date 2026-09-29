namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

/// <summary>
///     Реализует встроеннные методы ICRUDTenantProfileAppService для создания, обновления, удаления записей.
/// </summary>
[Authorize(TenantProfilePermissions.GroupName)]
public class CRUDTenantProfileAppService :
    CrudAppService<
        TenantProfile, //Сущность профиля тенанта
        TenantProfileDto, //DTO профиля тенанта
        Guid, //Первичный ключ
        PagedAndSortedResultRequestDto, //DTO использующийся для сортировки/отображения страниц
        CreateUpdateTenantProfileDto>, //DTO для создания/редактирования профиля
    ICRUDTenantProfileAppService
{
    public CRUDTenantProfileAppService(IRepository<TenantProfile, Guid> repository)
        : base(repository)
    {
    }

    [Authorize(TenantProfilePermissions.Profiles.Delete)]
    public override Task DeleteAsync(Guid id) => base.DeleteAsync(id);

    [Authorize(TenantProfilePermissions.Profiles.Get)]
    public override Task<TenantProfileDto> GetAsync(Guid id) => base.GetAsync(id);

    [Authorize(TenantProfilePermissions.Profiles.Get)]
    public override Task<PagedResultDto<TenantProfileDto>> GetListAsync(PagedAndSortedResultRequestDto input) => base.GetListAsync(input);

    [Authorize(TenantProfilePermissions.Profiles.Edit)]
    public override Task<TenantProfileDto> UpdateAsync(Guid id, CreateUpdateTenantProfileDto input) => base.UpdateAsync(id, input);

    [Authorize(TenantProfilePermissions.Profiles.Create)]
    public override Task<TenantProfileDto> CreateAsync(CreateUpdateTenantProfileDto input) => base.CreateAsync(input);

    [Authorize(TenantProfilePermissions.Profiles.Get)]
    public async Task<List<TenantProfileDto>> GetTenantProfileListAsync()
    {
        var tenants = await this.Repository.ToListAsync();
        var dtos = this.ObjectMapper.Map<List<TenantProfile>, List<TenantProfileDto>>(tenants);
        return dtos;
    }
}
