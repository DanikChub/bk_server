namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Dtos.Users;
using KV.Server.Employees;
using KV.Server.Helpers;
using KV.Server.Permissions;
using KV.Server.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

public class UsersAppService : ApplicationService, IUsersAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<CustomerUserProfile, Guid> _userRepository;
    private readonly IdentityUserAppService _identityUserAppService;
    private readonly IRepository<IdentityUser, Guid> _identityUserRepository;
    private readonly IRepository<TenantProfile, Guid> _customerUserRepository;
    private readonly IRepository<EmployeeProfile, Guid> _employeeRepository;
    public UsersAppService(IRepository<CustomerUserProfile, Guid> userRepository,
        IDataFilter dataFilter,
        IdentityUserAppService identityUserAppService,
        IRepository<IdentityUser, Guid> identityUserRepository,
        IRepository<TenantProfile, Guid> customerUserRepository,
        IRepository<EmployeeProfile, Guid> employeeRepository)
    {
        this._userRepository = userRepository;
        this._dataFilter = dataFilter;
        this._identityUserAppService = identityUserAppService;
        this._identityUserRepository = identityUserRepository;
        _customerUserRepository = customerUserRepository;
        _employeeRepository = employeeRepository;
    }

    public const string IsActiveLowerCaseName = "active";
    public const string IsBlockLowerCaseName = "block";

    [Authorize(UserPermissions.Profiles.Get)]
    public async Task<PagedResultDto<UserDto>> GetListAsync(GetUserListRequestDto input)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            //input.SearchResponsibleManagerId = CurrentUser.Id.ToString();
            var query = await this._userRepository.WithDetailsAsync(
                x => x.TenantProfile,
                x => x.TenantProfile.Contracts,
                x => x.IdentityUser);
            var (LastName, FirstName, MiddleName) = GetFullNameByString.GetFioByFullName(input.SearchFullName?.ToLower());
            if (!string.IsNullOrWhiteSpace(FirstName))
            {
                query = query.Where(x => x.FirstName.ToLower().Contains(FirstName));
            }
            if (!string.IsNullOrWhiteSpace(input.SearchResponsibleManagerId))
            {
                var employee = (await _employeeRepository.GetQueryableAsync())
                                .FirstOrDefault(x => x.IdentityUserId == Guid.Parse(input.SearchResponsibleManagerId));

                if (employee != null)
                {
                    query = query.Where(x => x.TenantProfile.ResponsibleManagerId == employee.Id);
                }
            }
            if (!string.IsNullOrWhiteSpace(LastName))
            {
                query = query.Where(x => x.LastName.ToLower().Contains(LastName));
            }

            if (!string.IsNullOrWhiteSpace(MiddleName))
            {
                query = query.Where(x => x.MiddleName.ToLower().Contains(MiddleName));
            }

            if (!string.IsNullOrWhiteSpace(input.SearchJobPost))
            {
                query = query.Where(x => x.JobPost.ToLower().Contains(input.SearchJobPost.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(input.SearchTenantShortName))
            {
                query = query.Where(x =>
                    x.TenantProfile.ShortName.ToLower().Contains(input.SearchTenantShortName.Trim().ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(input.SearchByActive))
            {
                if (input.SearchByActive == IsBlockLowerCaseName)
                {
                    query = query.Where(x => x.IdentityUser.IsActive == false);
                }
                else if (input.SearchByActive == IsActiveLowerCaseName)
                {
                    query = query.Where(x => x.TenantProfile.Contracts.Any());
                }
                else
                {
                    query = query.Where(x => !x.TenantProfile.Contracts.Any());
                }
            }

            if (input.SearchActualizationTime != null)
            {
                query = query.Where(x => x.LastModificationTime.Value.Date == input.SearchActualizationTime.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(input.SearchStr))
            {
                var searchStrFullName = GetFullNameByString.GetFioByFullName(input.SearchStr?.ToLower());
                query = query.Where(x =>
                    x.LastName.ToLower().Contains(searchStrFullName.LastName) ||
                    x.JobPost.ToLower().Contains(input.SearchStr.Trim().ToLower()) ||
                    x.TenantProfile.ShortName.ToLower().Contains(input.SearchStr.Trim().ToLower()));
            }

            if (!string.IsNullOrEmpty(input.Sorting))
            {
                if (input.Sorting == "fullName asc")
                {
                    query = query.OrderBy(x => x.LastName);
                }
                else if (input.Sorting == "fullName desc")
                {
                    query = query.OrderByDescending(x => x.LastName);
                }
                else if (input.Sorting == "direction asc")
                {
                }
                else if (input.Sorting == "direction desc")
                {
                }
                else if (input.Sorting == "jobPost asc")
                {
                    query = query.OrderBy(x => x.JobPost);
                }
                else if (input.Sorting == "jobPost desc")
                {
                    query = query.OrderByDescending(x => x.JobPost);
                }
                else if (input.Sorting == "tenantShortName asc")
                {
                    query = query.OrderBy(x => x.TenantProfile.ShortName);
                }
                else if (input.Sorting == "tenantShortName desc")
                {
                    query = query.OrderByDescending(x => x.TenantProfile.ShortName);
                }
                else if (input.Sorting == "actualizationTime asc")
                {
                    query = query.OrderBy(x => x.LastModificationTime);
                }
                else if (input.Sorting == "actualizationTime desc")
                {
                    query = query.OrderByDescending(x => x.LastModificationTime);
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
            var entities = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount == 0
                    ? GetCustomerEmployeeListRequestDto.DefaultPageSize
                    : input.MaxResultCount).ToList();
            var dtos = this.ObjectMapper.Map<List<CustomerUserProfile>, List<UserDto>>(entities);
            return new PagedResultDto<UserDto>(totalCount, dtos);
        }
    }

    [Authorize(UserPermissions.Profiles.Delete)]
    public async Task BlockUserAsync(Guid userId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var user = (await this._userRepository
                .WithDetailsAsync(x => x.IdentityUser))
                .FirstOrDefault(x => x.Id == userId);
            user.IdentityUser.SetIsActive(false);
            await _identityUserRepository.UpdateAsync(user.IdentityUser);
            await this._userRepository.UpdateAsync(user, true);
        }
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var user = (await this._userRepository
                .WithDetailsAsync(x => x.IdentityUser))
                .FirstOrDefault(x => x.Id == userId);
            if (user != null)
            {
                await this._userRepository.DeleteAsync(user);

            }
        }
    }

    public async Task CreateUserByTenantIdAsync(CreateUpdateUserDto dto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var identityUserDto = await this._identityUserAppService.CreateAsync(new IdentityUserCreateDto
            {
                Email = dto.Email,
                Surname = dto.LastName,
                Name = dto.FirstName,
                Password = dto.Password,
                UserName = dto.UserName,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true
            });
            var newIdentityUser = await this._identityUserRepository.FirstOrDefaultAsync(x => x.Id == identityUserDto.Id);
            newIdentityUser.SetTenantId(dto.TenantId);
            newIdentityUser.SetIsActive(true);
            await this._identityUserRepository.UpdateAsync(newIdentityUser);
            await this.CurrentUnitOfWork.SaveChangesAsync();
            var employee = new CustomerUserProfile(newIdentityUser.Id, newIdentityUser.Id, null,
                dto.TenantId);
            employee.SetFullName(dto.LastName ?? "", dto.FirstName ?? "", dto.MiddleName ?? "");
            employee.SetJobPost(dto.JobPost);
            employee.SetDateOfBirthday(dto.DateOfBirth);
            await this._userRepository.InsertAsync(employee);
        }
    }

    public async Task UnBlockUserAsync(Guid userId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var user = (await this._userRepository
                .WithDetailsAsync(x => x.IdentityUser))
                .FirstOrDefault(x => x.Id == userId);
            user.IdentityUser.SetIsActive(true);
            await _identityUserRepository.UpdateAsync(user.IdentityUser);
            await this._userRepository.UpdateAsync(user, true);
        }
    }

    public async Task<CustomerUserProfileDto> GetUserByIdAsync(Guid? userId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var user = (await this._userRepository
                .WithDetailsAsync(x => x.IdentityUser))
                .FirstOrDefault(x => x.Id == userId);
            return ObjectMapper.Map<CustomerUserProfile, CustomerUserProfileDto>(user);
        }
    }
}
