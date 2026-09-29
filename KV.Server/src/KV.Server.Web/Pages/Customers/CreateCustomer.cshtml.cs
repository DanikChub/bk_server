namespace KV.Server.Web.Pages.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KV.Server.Helpers;
using KV.Server.Interfaces;
using KV.Server.Localization;
using KV.Server.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

[Authorize]
public class CreateCustomerModel : ServerPageModel
{
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly IStringLocalizer<ServerResource> _stringLocalizer;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<Region, Guid> _regionRepository;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IEmployeeProfileAppService _employeeProfileAppService;

    public CreateCustomerModel(ICrudCustomerAppService crudCustomerAppService,
        IStringLocalizer<ServerResource> stringLocalizer,
        IRepository<Tenant, Guid> tenantRepository,
        IRepository<Region, Guid> regionRepository,
        ITenantManager tenantManager,
        IGuidGenerator guidGenerator,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        IUnitOfWorkManager unitOfWorkManager,
        IEmployeeProfileAppService employeeProfileAppService)
    {
        this._crudCustomerAppService = crudCustomerAppService;
        _stringLocalizer = stringLocalizer;
        this._tenantRepository = tenantRepository;
        this._regionRepository = regionRepository;
        this._tenantManager = tenantManager;
        this._guidGenerator = guidGenerator;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._unitOfWorkManager = unitOfWorkManager;
        this.Customer = new CreateCustomerViewModal();
        _employeeProfileAppService = employeeProfileAppService;
    }

    [BindProperty] public CreateCustomerViewModal Customer { get; set; }

    public List<SelectListItem> Regions { get; set; }

    public List<SelectListItem> Responsibles { get; set; }
    protected async Task Init()
    {
        var regions = await this._regionRepository.ToListAsync();
        this.Regions = new List<SelectListItem>();
        foreach (var region in regions)
        {
            this.Regions.Add(new SelectListItem { Text = region.Name, Value = region.Id.ToString() });
        }

        var specialists = await this._employeeProfileAppService.GetAllEmployeeAsync();
        this.Responsibles = new List<SelectListItem>();
        foreach (var specialist in specialists)
        {
            this.Responsibles.Add(new SelectListItem
            {
                Text = $"{specialist.LastName} {specialist.FirstName} {specialist.MiddleName}",
                Value = specialist.Id.ToString()
            });
        }
    }
    public async Task OnGetAsync()
    {
        await Init();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            //ValidateModel();
            var newTenantName = this._guidGenerator.Create().ToString();
            var (LastName, FirstName, MiddleName) = GetFullNameByString.GetFioByFullName(this.Customer.AdminFullName);
            Guid? tenantProfileId = null;
            using (var uow = this._unitOfWorkManager.Begin(isTransactional: true))
            {
                var tenant = await this._tenantManager.CreateAsync(newTenantName);
                await this._tenantRepository.InsertAsync(tenant);
                var tenantProfileDto = this.ObjectMapper.Map<CreateCustomerViewModal, CreateUpdateCustomerDto>(this.Customer);
                tenantProfileId = tenant.Id;
                tenantProfileDto.Id = tenant.Id;
                tenantProfileDto.TenantId = tenant.Id;
                tenantProfileDto.IsActive = true;
                tenantProfileDto.ResponsibleManagerId = this.Customer.ResponsibleId;
                tenantProfileDto.DisplayName = tenantProfileDto.ShortName;
                tenantProfileDto.PhoneNumber = tenantProfileDto.PhoneNumber;
                var createdCustomer = await this._crudCustomerAppService.CreateAsync(tenantProfileDto);

                await uow.SaveChangesAsync();

                var identityUserId = this._guidGenerator.Create();
                var createCustomerUserProfile = new CreateUpdateCustomerUserProfileDto
                {
                    Id = identityUserId,
                    IdentityUserId = identityUserId,
                    Email = this.Customer.AdminEmailAddress,
                    Login = this.Customer.AdminLogin,
                    JobPost = this.Customer.AdminJobPost,
                    DateOfBirthDay = this.Customer.AdminDateOfBirthDay,
                    LastName = LastName,
                    FirstName = FirstName,
                    MiddleName = MiddleName,
                    Password = this.Customer.AdminPassword,
                    TenantId = tenant.Id,
                    CreatorId = this.CurrentUser.Id
                };
                var customerUser = await this._customerUserProfilesAppService
                    .CreateWithIdentityUserAsync(
                        createCustomerUserProfile);

                await uow.SaveChangesAsync();

                await _crudCustomerAppService.UpdateContactUserProfileAsync(customerUser.Id, createdCustomer.Id);

                await uow.CompleteAsync();
            }

            return this.Redirect($"/Customers/Details/{tenantProfileId}");
        }
        catch (AbpIdentityResultException exc)
        {
            var text = _stringLocalizer.GetString(exc.Code);

            if(!text.ResourceNotFound)
            { 
                Alerts.Danger(text);
            }
            else
            {
                Alerts.Danger(exc.Message);
            }
            await Init();
            return Page();
        }
        catch (UserFriendlyException exc)
        {
            Alerts.Danger(exc.Message);
            await Init();
            return Page();
        }
    }
}

public class CreateCustomerViewModal : IValidatableObject
{
    [Required][MaxLength(1000)] public string ShortName { get; set; }

    [Required][MaxLength(2000)] public string LongName { get; set; }

    [Required][MaxLength(2000)] public string Address { get; set; }

    [Required] public string DisplayName { get; set; }

    public string SiteUrl { get; set; }
    public string ContractFullName { get; set; }
    public string ContractEmail { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public string ContractPhoneNumber { get; set; }
    public string Description { get; set; }
    public string INNNumber { get; set; }
    public string KPPNumber { get; set; }
    public string CommentNotes { get; set; }

    public Guid? RegionId { get; set; }

    public Guid? ResponsibleId { get; set; }

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string AdminEmailAddress { get; set; }

    [Required] public string AdminFullName { get; set; }

    [Required] public string AdminJobPost { get; set; }

    [DataType(DataType.Date)] public DateTime? AdminDateOfBirthDay { get; set; } = DateTime.Now.AddYears(-30);

    [MaxLength(128)] public string AdminPhoneNumber { get; set; }

    [Required][MaxLength(256)] public string AdminLogin { get; set; }

    [Required(ErrorMessage = "Пароль обязателен для заполнения")]
    [MinLength(3, ErrorMessage = "Пароль должен содержать как минимум 3 символа")]
    [MaxLength(128, ErrorMessage = "Пароль не должен превышать 128 символов")]
    [DataType(DataType.Password)]
    [RegularExpression("^.{3,}$", ErrorMessage = "Пароль должен содержать минимум 3 символа")]
    public string AdminPassword { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var service = validationContext.GetRequiredService<IdentityUserManager>();
        var validations = new List<ValidationResult>();
        var validators = service.PasswordValidators;

        if (this.AdminEmailAddress == this.AdminPassword)
        {
            validations.Add(new ValidationResult("Email and Password can not be the same!"));
        }

        return validations;
    }
}
