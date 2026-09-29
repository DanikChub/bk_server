using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Dtos.Employees;
using KV.Server.Dtos.File;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
namespace KV.Server.Interfaces;
public interface IEmployeeProfileAppService : IApplicationService
{
    Task<PagedResultDto<EmployeeProfileDto>> GetListAsync(GetEmployeeProfileListRequestDto input);
    Task<EmployeeProfileDto> CreateEmployeeAsync(CreateUpdateEmployeeProfileDto updateDto);
    Task<List<EmployeeProfileDto>> GetAllEmployeeAsync();
    Task<ListResultDto<EmployeeProfileDto>> GetActiveEmployeesListAsync();
    Task<EmployeeProfileDto> UpdateEmployeeAsync(Guid employeeId, CreateUpdateEmployeeProfileDto updateDto);
    Task UploadEmployeeAvatarAsync(Guid employeeId, IRemoteStreamContent request);
    Task<UploadedFileDto> GetAvatarByEmployeeIdAsync(Guid employeeId);
    Task<EmployeeProfileDto> GetEmployeeByIdAsync(Guid employeeId);
    Task<EmployeeProfileDto> GetEmployeeByUserNameAsync(string userName);
    Task<string> GetEmployeePasswordAsync(Guid employeeId);
    Task<List<string>> GetRoleNamesByEmployeeIdAsync(Guid employeeId);
    Task<EmployeeProfileDto> GetEmployeeByIdentityIdAsync(Guid? indentityUserId);
}
