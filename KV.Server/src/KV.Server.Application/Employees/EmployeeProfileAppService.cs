using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Dtos.Employees;
using KV.Server.Dtos.File;
using KV.Server.File;
using KV.Server.Interfaces;
using KV.Server.Permissions;
using KV.Server.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;

namespace KV.Server.Employees;
public class EmployeeProfileAppService : ApplicationService, IEmployeeProfileAppService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly IRepository<EmployeeProfile, Guid> _employeeProfileRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<UploadedFile, Guid> _uploadedFileRepository;
    private readonly IStringEncryptionService _encryptionService;
    private readonly IBlobContainer _blobContainer;

    private const string EncryptedPasswordPropertyName = "EncryptedPassword";
    private const string IdentityUserEmailDomain = "@kv.system";

    public EmployeeProfileAppService(UserManager<IdentityUser> userManager,
         IConfiguration configuration,
         IOptions<IdentityOptions> identityOptions,
         IRepository<EmployeeProfile,Guid> employeeProfileRepository,
         IDataFilter dataFilter, 
         IRepository<UploadedFile, Guid> uploadedFileRepository,
         IBlobContainerFactory blobContainerFactory,
         IStringEncryptionService encryptionService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _identityOptions = identityOptions;
        _employeeProfileRepository = employeeProfileRepository;
        _dataFilter = dataFilter;
        _uploadedFileRepository = uploadedFileRepository;
        _encryptionService = encryptionService;
        _blobContainer = blobContainerFactory.Create(BlobContainers.DEFAULT_PUBLIC);
    }
    [Authorize(EmployeeProfilePermissions.Profiles.Create)]
    public async Task<EmployeeProfileDto> CreateEmployeeAsync(CreateUpdateEmployeeProfileDto updateDto)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var existingUserByName = await _userManager.FindByNameAsync(updateDto.Login);
            if (existingUserByName is not null)
            {
                throw new UserFriendlyException("Такой пользователь уже существует");
            }

            await _identityOptions.SetAsync();

            var user = new IdentityUser(
                    GuidGenerator.Create(),
                    updateDto.Login,
                    updateDto.Login + IdentityUserEmailDomain
                );
            (await _userManager.CreateAsync(user, updateDto.Password)).CheckErrors();
            user.Name = updateDto.FirstName;
            user.Surname = updateDto.LastName;
            (await _userManager.UpdateAsync(user)).CheckErrors();
            user.SetIsActive(true);

            await CurrentUnitOfWork.SaveChangesAsync();

            foreach (var item in updateDto.RoleNames)
            {
                await _userManager.AddToRoleAsync(user, item);
            }

            var employee = new EmployeeProfile(user.Id,
                            updateDto.UserAvatarFileId, updateDto.JobPost, updateDto.Phone);

            employee.SetFullName(updateDto.LastName ?? "", updateDto.FirstName ?? "", updateDto.MiddleName ?? "");
            employee.SetDateOfBirthday(updateDto.Birthday);
            employee.SetUserId(user.Id);
            employee.SetEmail(updateDto.Email);

            var encryptedPassword = _encryptionService.Encrypt(updateDto.Password, _configuration["PasswordEncryption:Key"]);
            employee.SetProperty(EncryptedPasswordPropertyName, encryptedPassword);

            var entity = await this._employeeProfileRepository.InsertAsync(employee);
            await CurrentUnitOfWork.SaveChangesAsync();
            var dto = this.ObjectMapper.Map<EmployeeProfile, EmployeeProfileDto>(entity);

            return dto;
        }
    }
    [Authorize(EmployeeProfilePermissions.Profiles.Get)]
    public async Task<EmployeeProfileDto> GetEmployeeByIdAsync(Guid employeeId)
    {
        var employee = (await _employeeProfileRepository.WithDetailsAsync(x => x.IdentityUser)).FirstOrDefault(x => x.Id == employeeId);
        var dto = ObjectMapper.Map<EmployeeProfile, EmployeeProfileDto>(employee);
        return dto;
    }

    [Authorize(EmployeeProfilePermissions.Profiles.Get)]
    public async Task<EmployeeProfileDto> GetEmployeeByUserNameAsync(string userName)
    {
        var employee = (await _employeeProfileRepository.WithDetailsAsync(x => x.IdentityUser)).FirstOrDefault(x => string.Equals(x.IdentityUser.UserName, userName));
        var dto = ObjectMapper.Map<EmployeeProfile, EmployeeProfileDto>(employee);
        return dto;
    }

    public async Task<UploadedFileDto> GetAvatarByEmployeeIdAsync(Guid employeeId)
    {
        var employee = (await _employeeProfileRepository.WithDetailsAsync(x => x.UserAvatarFile)).FirstOrDefault(x => x.Id == employeeId);
        if (employee == null)
        {
            return null;
        }
        var uploadedFile = await _uploadedFileRepository.FindAsync(x => x.Id == employee.UserAvatarFileId);

        var dto = ObjectMapper.Map<UploadedFile, UploadedFileDto>(uploadedFile);

        return dto;
    }
    [Authorize(EmployeeProfilePermissions.Profiles.Get)]
    public async Task<PagedResultDto<EmployeeProfileDto>> GetListAsync(GetEmployeeProfileListRequestDto input)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._employeeProfileRepository.WithDetailsAsync(x => x.IdentityUser, x => x.UserAvatarFile);
            var hasNoSearchDateOfBirth = DateTime.TryParse(input.SearchDateOfBirthDay, out var searchDateOfBirthDay);
            query = query
                .Where(x => string.IsNullOrWhiteSpace(input.SearchFirstName) ||
                            x.FirstName.ToLower().Contains(input.SearchFirstName.Trim().ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchLastName) ||
                            x.LastName.ToLower().Contains(input.SearchLastName.Trim().ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchMiddleName) ||
                            x.MiddleName.ToLower().Contains(input.SearchMiddleName.Trim().ToLower()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchDateOfBirthDay) || hasNoSearchDateOfBirth ||
                            x.Birthday >= searchDateOfBirthDay)
                .Where(x => string.IsNullOrWhiteSpace(input.SearchJobPost) ||
                            x.JobPost.ToLower().Contains(input.SearchJobPost.Trim().ToLower()));
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                if (input.Sorting == "jobPost asc")
                {
                    query = query.OrderBy(x => x.JobPost);
                }
                else if (input.Sorting == "jobPost desc")
                {
                    query = query.OrderByDescending(x => x.JobPost);
                }
                else if (input.Sorting == "lastName desc")
                {
                    query = query.OrderByDescending(x => x.LastName);
                }
                else if (input.Sorting == "lastName asc")
                {
                    query = query.OrderBy(x => x.LastName);
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.CreationTime);
            }

            var totalCount = query.Count();
            var entities = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount == 0 ? GetContractListRequestDto.DefaultPageSize : input.MaxResultCount)
                .ToList();
            var dtos = this.ObjectMapper.Map<List<EmployeeProfile>, List<EmployeeProfileDto>>(entities);
            return new PagedResultDto<EmployeeProfileDto>(totalCount, dtos);
        }
    }
    [Authorize(EmployeeProfilePermissions.Profiles.Get)]
    public async Task<List<string>> GetRoleNamesByEmployeeIdAsync(Guid employeeId)
    {
        var employee = (await _employeeProfileRepository.WithDetailsAsync(x => x.UserAvatarFile, x => x.IdentityUser)).FirstOrDefault(x => x.Id == employeeId);
        var roles = (await _userManager.GetRolesAsync(employee.IdentityUser)).ToList();
        return roles;
    }
    [Authorize(EmployeeProfilePermissions.Profiles.Edit)]
    public async Task UploadEmployeeAvatarAsync(Guid employeeId, IRemoteStreamContent request)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var uploadedFile = new UploadedFile
            {
                FileName = request.FileName,
                ContentType = request.ContentType,
                SizeInBytes = request.ContentLength ?? 0,
                Title = request.FileName + " от " + string.Format("{0:dd.MM.yyyy HH:mm}", DateTime.Now)
            };
            await this._uploadedFileRepository.InsertAsync(uploadedFile);
            var employee = await _employeeProfileRepository.GetAsync(employeeId);
            employee.SetAvatar(uploadedFile.Id);
            await _employeeProfileRepository.UpdateAsync(employee, true);
            await this.CurrentUnitOfWork.SaveChangesAsync();

            await this._blobContainer.SaveAsync(uploadedFile.Id.ToString(), request.GetStream(), true);
        }
    }
    [Authorize(EmployeeProfilePermissions.Profiles.Edit)]
    public async Task<EmployeeProfileDto> UpdateEmployeeAsync(Guid employeeId, CreateUpdateEmployeeProfileDto updateDto)
    {
        var employee = (await _employeeProfileRepository.WithDetailsAsync(x => x.UserAvatarFile, x => x.IdentityUser)).FirstOrDefault(x => x.Id == employeeId);

        if (updateDto.Password is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(employee.IdentityUser);
            await _userManager.ResetPasswordAsync(employee.IdentityUser, token, updateDto.Password);
            var encryptedPassword = _encryptionService.Encrypt(updateDto.Password, _configuration["PasswordEncryption:Key"]);
            employee.SetProperty(EncryptedPasswordPropertyName, encryptedPassword);
        }

        employee.SetFullName(updateDto.LastName, updateDto.FirstName, updateDto.MiddleName);
        employee.SetJobPost(updateDto.JobPost);
        employee.SetDateOfBirthday(updateDto.Birthday);

        if (!string.IsNullOrEmpty(updateDto.Phone))
        {
            employee.SetPhone(updateDto.Phone);
        }

        if(!string.IsNullOrEmpty(updateDto.Email))
        {
            employee.SetEmail(updateDto.Email);
        }

        if (updateDto.RoleNames.Count() != 0)
        {
            var oldRoles = (await _userManager.GetRolesAsync(employee.IdentityUser)).ToList();
            await _userManager.RemoveFromRolesAsync(employee.IdentityUser, oldRoles);
            foreach (var item in updateDto.RoleNames)
            {
                await _userManager.AddToRoleAsync(employee.IdentityUser, item);
            }
        }

        var updatedEmployee = await _employeeProfileRepository.UpdateAsync(employee, true);
        var dto = ObjectMapper.Map<EmployeeProfile, EmployeeProfileDto>(updatedEmployee);
        return dto;
    }

    [Authorize(EmployeeProfilePermissions.Profiles.Create)]
    public async Task<string> GetEmployeePasswordAsync(Guid employeeId)
    {
        var employee = (await _employeeProfileRepository.WithDetailsAsync(x => x.IdentityUser))
            .FirstOrDefault(x => x.Id == employeeId);

        if (employee.HasProperty(EncryptedPasswordPropertyName))
        {
            var encryptedPassword = employee.GetProperty<string>(EncryptedPasswordPropertyName);

            if (!string.IsNullOrEmpty(encryptedPassword))
            {
                return _encryptionService.Decrypt(encryptedPassword, _configuration["PasswordEncryption:Key"]);
            }
        }

        return "Пароль не задан";
    }

    [Authorize(EmployeeProfilePermissions.Profiles.Get)]
    public async Task<List<EmployeeProfileDto>> GetAllEmployeeAsync()
    {
        var employees = (await _employeeProfileRepository.WithDetailsAsync()).ToList();

        var empolyeeDtos = ObjectMapper.Map<List<EmployeeProfile>, List<EmployeeProfileDto>>(employees);

        return empolyeeDtos;
    }

    [Authorize(EmployeeProfilePermissions.Profiles.Get)]
    public async Task<ListResultDto<EmployeeProfileDto>> GetActiveEmployeesListAsync()
    {
        var queryable = await _employeeProfileRepository.WithDetailsAsync();

        var employees = queryable.Where(x => x.IdentityUser.IsActive).ToList();

        return new ListResultDto<EmployeeProfileDto> { Items = ObjectMapper.Map<List<EmployeeProfile>, List<EmployeeProfileDto>>(employees) };
    }

    public async Task<EmployeeProfileDto> GetEmployeeByIdentityIdAsync(Guid? indentityUserId)
    {
        var employee = (await _employeeProfileRepository.WithDetailsAsync(x => x.IdentityUser)).FirstOrDefault(x => x.IdentityUserId == indentityUserId);
        var dto = ObjectMapper.Map<EmployeeProfile, EmployeeProfileDto>(employee);
        return dto;
    }
}
