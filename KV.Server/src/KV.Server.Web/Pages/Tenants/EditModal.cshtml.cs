namespace Server.Web.Pages.Tenants;
using System;
using System.Threading.Tasks;
using KV.Server;
using KV.Server.Web.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class EditModalModel : ServerPageModel
{
    private readonly ICRUDTenantProfileAppService _tenantAppService;

    public EditModalModel(ICRUDTenantProfileAppService tenantAppService)
    {
        this._tenantAppService = tenantAppService;
    }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid Id { get; set; }

    [BindProperty] public CreateUpdateTenantProfileDto Profile { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var profileDto = await this._tenantAppService.GetAsync(id);
        this.Id = id;
        this.Profile = this.ObjectMapper.Map<TenantProfileDto, CreateUpdateTenantProfileDto>(profileDto);
        return this.Page();
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        this.ValidateModel();
        await this._tenantAppService.UpdateAsync(this.Id, this.Profile);
        return this.NoContent();
    }
}
