namespace KV.Server.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.File;
using KV.Server.Interfaces;
using KV.Server.Permissions;
using KV.Server.Permissions.Customers;
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
using Volo.Abp.ObjectExtending;
using Volo.Abp.Security.Encryption;

public class CustomerUserProfilesAppService : ApplicationService, ICustomerUserProfilesAppService
{
    private readonly IRepository<CustomerUserProfile, Guid> _customerUserProfilesRepository;
    private readonly IConfiguration _configuration;
    private readonly IStringEncryptionService _encryptionService;
    private readonly IDataFilter _dataFilter;
    private readonly IIdentityUserRepository _identityUserManagerRepository;
    private readonly IRepository<IdentityUser, Guid> _identityUserRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly IRepository<UploadedFile, Guid> _uploadedFileRepository;
    private readonly IBlobContainer _blobContainer;
    private const string EncryptedPasswordPropertyName = "EncryptedPassword";
    private const string IdentityUserEmailDomain = "@kv.system";

    public CustomerUserProfilesAppService(IRepository<CustomerUserProfile, Guid> customerUserProfilesRepository,
        IConfiguration configuration,
        IStringEncryptionService encryptionService,
        IRepository<IdentityUser, Guid> identityUserRepository,
        IDataFilter dataFilter,
        IIdentityUserRepository identityUserManagerRepository,
        ICurrentTenant currentTenant,
        UserManager<IdentityUser> userManager,
        IOptions<IdentityOptions> identityOptions,
        ICrudCustomerAppService crudCustomerAppService,
        IRepository<UploadedFile, Guid> uploadedFileRepository,
        IBlobContainerFactory blobContainerFactory)
    {
        _customerUserProfilesRepository = customerUserProfilesRepository;
        _configuration = configuration;
        _encryptionService = encryptionService;
        _identityUserRepository = identityUserRepository;
        _dataFilter = dataFilter;
        _identityUserManagerRepository = identityUserManagerRepository;
        _currentTenant = currentTenant;
        _userManager = userManager;
        _identityOptions = identityOptions;
        _crudCustomerAppService = crudCustomerAppService;
        _uploadedFileRepository = uploadedFileRepository;
        _blobContainer = blobContainerFactory.Create(BlobContainers.DEFAULT_PUBLIC);
    }

    [Authorize(CustomerUserPermissions.CustomerUsers.Create)]
    public async Task<CustomerUserProfileDto> CreateWithIdentityUserAsync(CreateUpdateCustomerUserProfileDto create)
    {
        using (_currentTenant.Change(create.TenantId))
        {
            var existingUserByName = await _userManager.FindByNameAsync(create.Login);
            if (existingUserByName is not null)
            {
                throw new UserFriendlyException($"Пользователь {create.Login} уже существует");
            }

            await _identityOptions.SetAsync();
            var user = new IdentityUser(
                    GuidGenerator.Create(),
                    create.Login,
                    create.Login + IdentityUserEmailDomain,
                    create.TenantId
                );
            var createUserDto = new IdentityUserCreateDto
            {
                Email = create.Email,
                Surname = create.LastName,
                Name = create.FirstName,
                Password = create.Password,
                UserName = create.Login,
                PhoneNumber = create.PhoneNumber,
                IsActive = true
            };
            createUserDto.MapExtraPropertiesTo(user);
            (await _userManager.CreateAsync(user, create.Password)).CheckErrors();
            (await _userManager.SetPhoneNumberAsync(user, create.PhoneNumber)).CheckErrors();
            user.Name = create.FirstName;
            user.Surname = create.LastName;
            (await _userManager.UpdateAsync(user)).CheckErrors();
            user.SetIsActive(true);

            await CurrentUnitOfWork.SaveChangesAsync();


            try
            {
                await _crudCustomerAppService.CreateClientRoleAsync((Guid)create.TenantId);
            }
            finally
            {
                await _userManager.AddToRoleAsync(user, UserRoleConstants.CLIENT_ROLE);
            }

            var employee = new CustomerUserProfile(user.Id,
                        user.Id, create.UserAvatarFileId,
                        create.TenantId);
            employee.SetFullName(create.LastName ?? "", create.FirstName ?? "", create.MiddleName ?? "");
            employee.SetJobPost(create.JobPost);
            employee.SetDateOfBirthday(create.DateOfBirthDay);
            employee.SetEmail(create.Email);
            var encryptedPassword = _encryptionService.Encrypt(create.Password, _configuration["PasswordEncryption:Key"]);
            employee.SetProperty(EncryptedPasswordPropertyName, encryptedPassword);
            var entity = await this._customerUserProfilesRepository.InsertAsync(employee);
            var dto = this.ObjectMapper.Map<CustomerUserProfile, CustomerUserProfileDto>(entity);

            return dto;
        }
    }

    public async Task UploadAvatarAsync(Guid customerUserId, IRemoteStreamContent request)
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

            await _uploadedFileRepository.InsertAsync(uploadedFile);

            var customerUser = await _customerUserProfilesRepository.FirstOrDefaultAsync(x => x.Id == customerUserId);
            customerUser.SetAvatar(uploadedFile.Id);
            await _customerUserProfilesRepository.UpdateAsync(customerUser, true);
            await this.CurrentUnitOfWork.SaveChangesAsync();

            await this._blobContainer.SaveAsync(uploadedFile.Id.ToString(), request.GetStream(), true);
        }
    }

    public async Task UpdateAsync(CreateUpdateCustomerUserProfileDto update)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var employee = await this._customerUserProfilesRepository.FirstOrDefaultAsync(x => x.Id == update.Id);
            employee.SetJobPost(update.JobPost);
            employee.SetFullName(update.LastName ?? "", update.FirstName ?? "", update.MiddleName);
            employee.SetAddress(update.Country, update.City, update.Street);
            employee.WriteNote(update.Note);
            employee.SetDateOfBirthday(update.DateOfBirthDay);

            if(!string.IsNullOrEmpty(update.Email))
            {
                employee.SetEmail(update.Email);
            }

            await this._customerUserProfilesRepository.UpdateAsync(employee, true);
            var identityUser = await this._identityUserRepository.FirstOrDefaultAsync(x => x.Id == employee.IdentityUserId);
            identityUser.SetPhoneNumber(update.PhoneNumber, false);
            await this._identityUserRepository.UpdateAsync(identityUser, true);
        }
    }

    [Authorize(CustomerUserPermissions.CustomerUsers.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var employee = await this._customerUserProfilesRepository.FirstOrDefaultAsync(x => x.Id == id);
            await this._customerUserProfilesRepository.DeleteAsync(employee);
            await this._identityUserRepository.DeleteAsync(employee.IdentityUserId);
        }
    }

    [Authorize(CustomerUserPermissions.CustomerUsers.Create)]
    public async Task<CustomerUserProfileDto> CreateByIdentityIdAsync(Guid identityUserId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var identityUser = await this._identityUserRepository.FirstOrDefaultAsync(x => x.Id == identityUserId);
            var customerUserProfile =
                new CustomerUserProfile(identityUserId, identityUserId, null, identityUser.TenantId);
            customerUserProfile.SetFullName(identityUser.Surname ?? "", identityUser.Name ?? "", "");
            var entity = await this._customerUserProfilesRepository.InsertAsync(customerUserProfile);
            var dto = this.ObjectMapper.Map<CustomerUserProfile, CustomerUserProfileDto>(entity);
            return dto;
        }
    }

    [Authorize(CustomerUserPermissions.CustomerUsers.Create)]
    public async Task<CustomerUserProfileDto> CreateAsync(CreateUpdateCustomerUserProfileDto create)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var customerUserProfile = new CustomerUserProfile(create.Id, create.IdentityUserId, create.UserAvatarFileId,
                create.TenantId);
            customerUserProfile.SetFullName(create.LastName ?? "", create.FirstName ?? "", create.MiddleName ?? "");
            customerUserProfile.SetDateOfBirthday(create.DateOfBirthDay);
            customerUserProfile.SetJobPost(create.JobPost);
            customerUserProfile.SetAddress(create.Country, create.City, create.Street);
            customerUserProfile.WriteNote(create.Note);
            var entity = await this._customerUserProfilesRepository.InsertAsync(customerUserProfile);
            var dto = this.ObjectMapper.Map<CustomerUserProfile, CustomerUserProfileDto>(entity);
            return dto;
        }
    }

    [Authorize(CustomerUserPermissions.CustomerUsers.Get)]
    public async Task<PagedResultDto<CustomerUserProfileDto>> GetListAsync(GetCustomerEmployeeListRequestDto input)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._customerUserProfilesRepository.WithDetailsAsync(x => x.TenantProfile, x => x.IdentityUser);
            query = query.Where(x => x.TenantProfile.Id == input.CustomerId);
            var hasNoSearchDateOfBirth = DateTime.TryParse(input.SearchDateOfBirthDay, out var searchDateOfBirthDay);
            query = query
                .Where(x => string.IsNullOrWhiteSpace(input.SearchFirstName) ||
                            x.FirstName.ToLowerInvariant().Contains(input.SearchFirstName.Trim().ToLowerInvariant()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchLastName) ||
                            x.LastName.ToLowerInvariant().Contains(input.SearchLastName.Trim().ToLowerInvariant()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchMiddleName) ||
                            x.MiddleName.ToLowerInvariant().Contains(input.SearchMiddleName.Trim().ToLowerInvariant()))
                .Where(x => string.IsNullOrWhiteSpace(input.SearchDateOfBirthDay) || hasNoSearchDateOfBirth ||
                            x.DateOfBirthDay >= searchDateOfBirthDay)
                .Where(x => string.IsNullOrWhiteSpace(input.SearchJobPost) ||
                            x.JobPost.ToLowerInvariant().Contains(input.SearchJobPost.Trim().ToLowerInvariant()));
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                if (input.Sorting == "responsibleFullName asc")
                {
                    query = query.OrderBy(x => x.LastName);
                }
                else if (input.Sorting == "responsibleFullName desc")
                {
                    query = query.OrderByDescending(x => x.LastName);
                }
                else if (input.Sorting == "jobPost asc")
                {
                    query = query.OrderBy(x => x.JobPost);
                }
                else if (input.Sorting == "jobPost desc")
                {
                    query = query.OrderByDescending(x => x.JobPost);
                }
                else if (input.Sorting == "shortName asc") // Направление деятельности
                {
                    query = query.OrderBy(x => x.TenantProfile.ShortName);
                }
                else if (input.Sorting == "shortName desc")
                {
                    query = query.OrderByDescending(x => x.TenantProfile.ShortName);
                }
                else if (input.Sorting == "contractStartDate asc") // актуализация
                {
                    query = query.OrderBy(x => x.LastModificationTime);
                }
                else if (input.Sorting == "contractStartDate desc")
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
                .Take(input.MaxResultCount == 0 ? GetContractListRequestDto.DefaultPageSize : input.MaxResultCount)
                .ToList();
            var dtos = this.ObjectMapper.Map<List<CustomerUserProfile>, List<CustomerUserProfileDto>>(entities);
            return new PagedResultDto<CustomerUserProfileDto>(totalCount, dtos);
        }
    }

    [Authorize(CustomerPermissions.Profiles.Get)]
    public async Task<List<CustomerUserProfileDto>> GetCustomerDateOfBirthdayUsersAsync(DateTime start, DateTime end)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var customers = (await this._customerUserProfilesRepository
                    .WithDetailsAsync(x => x.TenantProfile))
                .Where(x => x.DateOfBirthDay != null)
                .ToList();

            var result = new List<CustomerUserProfile>();
            foreach (var customer in customers)
            {
                if (customer.DateOfBirthDay != null)
                {
                    var date = customer?.DateOfBirthDay ?? DateTime.Now;
                    date = date.AddYears(DateTime.Now.Year - date.Year);
                    if (date.IsBetween(start, end))
                    {
                        result.Add(customer);
                    }
                }
            }

            return this.ObjectMapper.Map<List<CustomerUserProfile>, List<CustomerUserProfileDto>>(result);
        }
    }

    [Authorize(CustomerUserPermissions.CustomerUsers.Get)]
    public async Task<List<CustomerUserProfileDto>> GetCustomerUserProfileListAsync()
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var customers = (await this._customerUserProfilesRepository
                    .WithDetailsAsync(x => x.IdentityUser))
                .ToList();
            var dtos = this.ObjectMapper.Map<List<CustomerUserProfile>, List<CustomerUserProfileDto>>(customers);
            return dtos;
        }
    }

    [Authorize(CustomerUserPermissions.CustomerUsers.Get)]
    public async Task<List<CustomerUserProfileDto>> GetSpecialistsAsync()
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var query = await this._customerUserProfilesRepository.WithDetailsAsync(x => x.IdentityUser);
            query = query.Where(x => x.TenantId == null);
            var specialists = query.ToList();
            var dtos = this.ObjectMapper.Map<List<CustomerUserProfile>, List<CustomerUserProfileDto>>(specialists);
            return dtos;
        }
    }

    public async Task<CustomerUserProfileDto> GetCustomerByUserNameAsync(string userName)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var customer = (await this._customerUserProfilesRepository
                    .WithDetailsAsync(x => x.IdentityUser, x => x.TenantProfile, x => x.UserAvatarFile))
                .FirstOrDefault(x => x.IdentityUser.UserName == userName);
            return this.ObjectMapper.Map<CustomerUserProfile, CustomerUserProfileDto>(customer);
        }
    }

    public async Task<CustomerUserProfileDto> GetAsync(Guid id)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var customer = (await this._customerUserProfilesRepository
                    .WithDetailsAsync(x => x.IdentityUser, x => x.TenantProfile, x => x.UserAvatarFile))
                .FirstOrDefault(x => x.Id == id);
            var dto = this.ObjectMapper.Map<CustomerUserProfile, CustomerUserProfileDto>(customer);
            dto.PhoneNumber = customer?.IdentityUser?.PhoneNumber;
            return dto;
        }
    }

    [Authorize(EmployeeProfilePermissions.Profiles.Create)]
    public async Task<string> GetUserPasswordAsync(Guid userId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var user = (await _customerUserProfilesRepository.WithDetailsAsync(x => x.IdentityUser))
            .FirstOrDefault(x => x.Id == userId);

            if (user.HasProperty(EncryptedPasswordPropertyName))
            {
                var encryptedPassword = user.GetProperty<string>(EncryptedPasswordPropertyName);

                if (!string.IsNullOrEmpty(encryptedPassword))
                {
                    return _encryptionService.Decrypt(encryptedPassword, _configuration["PasswordEncryption:Key"]);
                }
            }

            return "Пароль не задан";
        }
    }

    public async Task<CustomerUserProfileDto> GetCustomerByIdentityUserIdAsync(Guid identityUserId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var customer = (await this._customerUserProfilesRepository
                    .WithDetailsAsync(x => x.IdentityUser, x => x.TenantProfile, x => x.UserAvatarFile))
                .FirstOrDefault(x => x.IdentityUser.Id == identityUserId);
            return this.ObjectMapper.Map<CustomerUserProfile, CustomerUserProfileDto>(customer);
        }
    }

    public async Task<List<CustomerUserProfileDto>> GetColleaguesByIdentityUserIdAsync(Guid? identityUserId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var employee = await this._identityUserRepository.FirstOrDefaultAsync(x => x.Id == identityUserId);
            var colleagues = new List<CustomerUserProfile>();
            if (employee != null)
            {
                colleagues = (await this._customerUserProfilesRepository
                        .WithDetailsAsync(x => x.UserAvatarFile))
                    .Where(x => x.TenantId == employee.TenantId && x.Id != employee.Id)
                    .ToList();
            }

            return this.ObjectMapper.Map<List<CustomerUserProfile>, List<CustomerUserProfileDto>>(colleagues);
        }
    }

    [Authorize(CustomerUserPermissions.CustomerUsers.Get)]
    public async Task<List<IdentityUserDto>> GetIdentityUsersByTenantIdAsync(Guid tenantId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var identityUsers = (await this._identityUserRepository.GetQueryableAsync())
                .Where(x => x.TenantId == tenantId)
                .ToList();
            var dtos = this.ObjectMapper.Map<List<IdentityUser>, List<IdentityUserDto>>(identityUsers);
            return dtos;
        }
    }
    [Authorize(CustomerUserPermissions.CustomerUsers.Get)]
    public async Task<List<IdentityRoleDto>> GetRolesByIdAsync(Guid id)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var customerUserProfile = await this._customerUserProfilesRepository
                .FirstOrDefaultAsync(x => x.Id == id);
            var roles = await this._identityUserManagerRepository
                .GetRolesAsync(customerUserProfile.IdentityUserId);
            var dtos = this.ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(roles);
            return dtos;
        }
    }
    [Authorize(CustomerUserPermissions.CustomerUsers.Get)]
    public async Task<IdentityUserDto> GetIdentityUserByIdAsync(Guid userId)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var customerUserProfile = await this._customerUserProfilesRepository
                .FirstOrDefaultAsync(x => x.Id == userId);
            var user = await this._identityUserManagerRepository
                .GetAsync(customerUserProfile.IdentityUserId);
            var dto = this.ObjectMapper.Map<IdentityUser, IdentityUserDto>(user);
            return dto;
        }
    }
    [Authorize(CustomerUserPermissions.CustomerUsers.Edit)]
    public async Task ResetUserPasswordByIdAsync(string userId, string newPassword)
    {
        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var customerUserProfile = await this._customerUserProfilesRepository
                .FirstOrDefaultAsync(x => x.Id == userId.To<Guid>());

            var encryptedPassword = _encryptionService.Encrypt(newPassword, _configuration["PasswordEncryption:Key"]);
            customerUserProfile.SetProperty(EncryptedPasswordPropertyName, encryptedPassword);

            var user = await this._identityUserManagerRepository
                .GetAsync(customerUserProfile.IdentityUserId);
            var isPasswordRemoved = await _userManager.RemovePasswordAsync(user);
            var isPasswordAdded = await _userManager.AddPasswordAsync(user, newPassword);

            if (!isPasswordAdded.Succeeded)
            {
                throw new UserFriendlyException($"Пароль должен содержать цифры спец символ и заглавную букву");
            }
        }
    }
}
