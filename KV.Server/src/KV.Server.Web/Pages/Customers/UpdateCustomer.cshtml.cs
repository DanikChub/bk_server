namespace KV.Server.Web.Pages.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using KV.Server.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

[Authorize]
public class UpdateCustomerModel : ServerPageModel
{
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly IEmployeeProfileAppService _employeeProfileAppService;
    private readonly IRepository<Region, Guid> _regionRepository;

    public UpdateCustomerModel(ICrudCustomerAppService crudCustomerAppService,
        IRepository<Region, Guid> regionRepository,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        IEmployeeProfileAppService employeeProfileAppService)
    {
        this._crudCustomerAppService = crudCustomerAppService;
        this._regionRepository = regionRepository;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        _employeeProfileAppService = employeeProfileAppService;
    }

    [BindProperty] public UpdateCustomerViewModal Customer { get; set; }

    public List<SelectListItem> Regions { get; set; }

    public List<SelectListItem> Responsibles { get; set; }

    public List<SelectListItem> ContactUserProfiles { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        var customer = await this._crudCustomerAppService.GetAsync(id);
        this.Customer = this.ObjectMapper.Map<CustomerDto, UpdateCustomerViewModal>(customer);
        this.Customer.ResponsibleId = customer.ResponsibleManagerId;
        Customer.ContractEmail = customer.Email;
        Customer.ContractPhoneNumber = customer.PhoneNumber;
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

        var customerUsers = await _customerUserProfilesAppService.GetListAsync(new GetCustomerEmployeeListRequestDto { CustomerId = id, MaxResultCount = 100 });
        ContactUserProfiles = customerUsers.Items.Select(x =>
            new SelectListItem 
            { 
                Text = $"{x.LastName} {x.FirstName} {x.MiddleName}", 
                Value = x.Id.ToString(),
                Selected = (customer.ContractOwner is not null && customer.ContractOwner.Id  == x.Id)
            }
        ).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var tenantProfileDto = this.ObjectMapper.Map<UpdateCustomerViewModal, CreateUpdateCustomerDto>(this.Customer);
        tenantProfileDto.Id = this.Customer.Id;
        tenantProfileDto.TenantId = this.Customer.Id;
        tenantProfileDto.ResponsibleManagerId = this.Customer.ResponsibleId;
        tenantProfileDto.DisplayName = tenantProfileDto.ShortName;
        await this._crudCustomerAppService.UpdateAsync(this.Customer.Id, tenantProfileDto);

        if (Customer.ContactUserProfileId is not null)
        {
            await _crudCustomerAppService.UpdateContactUserProfileAsync(this.Customer.ContactUserProfileId, Customer.Id);
        }

        return this.Redirect($"/Customers/Details/{this.Customer.Id}");
    }
}

public class UpdateCustomerViewModal : IValidatableObject
{
    [HiddenInput] public Guid Id { get; set; }

    [Required][MaxLength(1000)] public string ShortName { get; set; }

    [Required][MaxLength(2000)] public string LongName { get; set; }

    [Required][MaxLength(2000)] public string Address { get; set; }

    [Required] public string DisplayName { get; set; }

    public string SiteUrl { get; set; }
    public string ContractFullName { get; set; }
    public string ContractEmail { get; set; }
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

    public Guid? ContactUserProfileId { get; set; }

    public string AdminJobPost { get; set; }

    [DataType(DataType.Date)] public DateTime? AdminDateOfBirthDay { get; set; } = DateTime.Now.AddYears(-30);

    [MaxLength(128)] public string AdminPhoneNumber { get; set; }

    [Required][MaxLength(256)] public string AdminLogin { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    [DataType(DataType.Password)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{8,}$",
        ErrorMessage =
            "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
    public string AdminPassword { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var service = validationContext.GetRequiredService<IdentityUserManager>();
        var validations = new List<ValidationResult>();
        var validators = service.PasswordValidators;
        //foreach (var validator in validators)
        //{
        //    var result = await validator.ValidateAsync(null, null, AdminPassword);
        //    if (!result.Succeeded)
        //    {
        //        foreach (var error in result.Errors)
        //        {
        //            validations.Add(new(error.Description));
        //        }
        //    }
        //}

        if (this.AdminEmailAddress == this.AdminPassword)
        {
            validations.Add(new ValidationResult("Email and Password can not be the same!"));
        }

        return validations;
    }
}
