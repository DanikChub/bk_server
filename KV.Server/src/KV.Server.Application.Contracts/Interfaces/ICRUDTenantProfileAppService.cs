namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

/// <summary>
///     Интерфейс для встроенного CRUD сервиса.
/// </summary>
public interface ICRUDTenantProfileAppService :
    ICrudAppService< //Стандартный C R U D (создать\обновить\удалить) интерфейс
        TenantProfileDto, //сущность передающая данные из TenantProfile
        Guid, //Первичный клюс
        PagedAndSortedResultRequestDto, //сущность используемая для сортировки\разбиения на страницы
        CreateUpdateTenantProfileDto> //сущность для первичного создания\обновления данных в TenantProfile
{
    Task<List<TenantProfileDto>> GetTenantProfileListAsync();
}
