namespace KV.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Dtos.Employees;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Identity;

public interface ICustomerUserProfilesAppService : IApplicationService
{
    Task<List<CustomerUserProfileDto>> GetCustomerUserProfileListAsync();
    Task<CustomerUserProfileDto> GetCustomerByUserNameAsync(string userName);
    Task<CustomerUserProfileDto> GetCustomerByIdentityUserIdAsync(Guid identityUserId);
    Task<List<CustomerUserProfileDto>> GetCustomerDateOfBirthdayUsersAsync(DateTime start, DateTime end);
    Task<List<CustomerUserProfileDto>> GetColleaguesByIdentityUserIdAsync(Guid? identityUserId);
    Task<PagedResultDto<CustomerUserProfileDto>> GetListAsync(GetCustomerEmployeeListRequestDto input);
    Task<CustomerUserProfileDto> CreateByIdentityIdAsync(Guid identityUserId);
    Task<CustomerUserProfileDto> CreateWithIdentityUserAsync(CreateUpdateCustomerUserProfileDto create);
    Task DeleteAsync(Guid id);
    Task<CustomerUserProfileDto> GetAsync(Guid id);
    Task<string> GetUserPasswordAsync(Guid userId);
    Task UpdateAsync(CreateUpdateCustomerUserProfileDto update);
    Task<CustomerUserProfileDto> CreateAsync(CreateUpdateCustomerUserProfileDto create);
    Task<List<IdentityUserDto>> GetIdentityUsersByTenantIdAsync(Guid tenantId);
    Task<IdentityUserDto> GetIdentityUserByIdAsync(Guid userId);
    Task<List<IdentityRoleDto>> GetRolesByIdAsync(Guid id);
    Task<List<CustomerUserProfileDto>> GetSpecialistsAsync();
    Task ResetUserPasswordByIdAsync(string userId, string newPassword);
    Task UploadAvatarAsync(Guid customerUserId, IRemoteStreamContent request);
}
