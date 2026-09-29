namespace KV.Server.Web.Pages.Tenants;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.TenantManagement;

[Authorize]
public class CreateModalModel : ServerPageModel
{
    private readonly ITenantAppService _tenantAppService;
    private readonly ICRUDTenantProfileAppService _tenantProfileCrudService;

    public CreateModalModel(ICRUDTenantProfileAppService tenantProfileCrudService,
        ITenantAppService tenantAppService)
    {
        this._tenantProfileCrudService = tenantProfileCrudService;
        this._tenantAppService = tenantAppService;
    }

    [BindProperty] public CreateTenantProfileViewModel TenantProfile { get; set; }

    public void OnGet() => this.TenantProfile = new CreateTenantProfileViewModel();

    public async Task<IActionResult> OnPostAsync()
    {
        this.ValidateModel();

        var requestTenantData = this.ObjectMapper.Map<CreateTenantProfileViewModel, TenantCreateDto>(this.TenantProfile);
        var requestTenantProfileData =
            this.ObjectMapper.Map<CreateTenantProfileViewModel, CreateUpdateTenantProfileDto>(this.TenantProfile);

        try
        {
            var tenantDto = await this._tenantAppService.CreateAsync(requestTenantData);
            requestTenantProfileData.Id = tenantDto.Id;
            await this._tenantProfileCrudService.CreateAsync(requestTenantProfileData);
        }
        catch
        {
            return this.BadRequest();
        }

        return this.NoContent();
    }
}

public class CreateTenantProfileViewModel
{
    [HiddenInput] public Guid Id { get; set; }

    [Required][DataType(DataType.Text)] public string ShortName { get; set; }

    [Required][DataType(DataType.Text)] public string LongName { get; set; }

    [Required][DataType(DataType.Text)] public string Address { get; set; }

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string AdminEmailAddress { get; set; }

    [Required]
    [MaxLength(128)]
    [DataType(DataType.Password)]
    public string AdminPassword { get; set; }
}
