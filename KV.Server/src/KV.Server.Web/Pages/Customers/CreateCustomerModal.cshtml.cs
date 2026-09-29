namespace KV.Server.Web.Pages.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

[Authorize]
public class CreateCustomerModalModel : ServerPageModel
{
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public CreateCustomerModalModel(ICrudCustomerAppService crudCustomerAppService,
        ITenantAppService tenantAppService,
        IRepository<Tenant, Guid> tenantRepository,
        ITenantManager tenantManager,
        IGuidGenerator guidGenerator,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        IUnitOfWorkManager unitOfWorkManager)
    {
        this._crudCustomerAppService = crudCustomerAppService;
        this._tenantRepository = tenantRepository;
        this._tenantManager = tenantManager;
        this._guidGenerator = guidGenerator;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._unitOfWorkManager = unitOfWorkManager;
    }

    [BindProperty] public CustomerModal Customer { get; set; }

    public void OnGet() => this.Customer = new CustomerModal();

    public async Task<IActionResult> OnPostAsync()
    {
        this.ValidateModel();
        var newTenantName = this._guidGenerator.Create().ToString();
        using (var uow = this._unitOfWorkManager.Begin(isTransactional: true))
        {
            var tenant = await this._tenantManager.CreateAsync(newTenantName);
            await this._tenantRepository.InsertAsync(tenant);
            var tenantProfileDto = this.ObjectMapper.Map<CustomerModal, CreateUpdateCustomerDto>(this.Customer);
            tenantProfileDto.Id = tenant.Id;
            tenantProfileDto.TenantId = tenant.Id;
            tenantProfileDto.IsActive = true;
            await this._crudCustomerAppService.CreateAsync(tenantProfileDto);
            var identityUserId = this._guidGenerator.Create();
            var createCustomerUserProfile = new CreateUpdateCustomerUserProfileDto
            {
                Id = identityUserId,
                IdentityUserId = identityUserId,
                Email = this.Customer.AdminEmailAddress,
                Login = this.Customer.AdminEmailAddress,
                Password = this.Customer.AdminPassword,
                TenantId = tenant.Id,
                CreatorId = this.CurrentUser.Id
            };
            await this._customerUserProfilesAppService.CreateWithIdentityUserAsync(createCustomerUserProfile);
            await uow.CompleteAsync();
        }

        return this.NoContent();
    }
}

public class CustomerModal : IValidatableObject
{
    [Required][MaxLength(1000)] public string ShortName { get; set; }

    [Required][MaxLength(2000)] public string LongName { get; set; }

    [Required][MaxLength(2000)] public string Address { get; set; }

    [Required] public string DisplayName { get; set; }

    public string SiteUrl { get; set; }
    public string ContractEmail { get; set; }
    public string ContractPhoneNumber { get; set; }
    public string Description { get; set; }
    public string INNNumber { get; set; }
    public string KPPNumber { get; set; }
    public string CommentNotes { get; set; }

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string AdminEmailAddress { get; set; }

    [Required]
    [MaxLength(128)]
    [DataType(DataType.Password)]
    public string AdminPassword { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var service = validationContext.GetRequiredService<IdentityUserManager>();
        var validations = new List<ValidationResult>();
        var validators = service.PasswordValidators;
        foreach (var validator in validators)
        {
            //var result = await validator.ValidateAsync(null, null, AdminPassword);
            //if (!result.Succeeded)
            //{
            //    foreach (var error in result.Errors)
            //    {
            //        validations.Add(new(error.Description));
            //    }
            //}
        }

        if (this.AdminEmailAddress == this.AdminPassword)
        {
            validations.Add(new ValidationResult("Email and Password can not be the same!"));
        }

        return validations;
    }
}
