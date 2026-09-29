namespace KV.Server.Web.Pages.Customers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class CreateManagerModalModel : ServerPageModel
{
    private readonly ICustomerUserManagerAppService _customerUserManagerAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;

    public CreateManagerModalModel(ICustomerUserManagerAppService customerUserManagerAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService)
    {
        this._customerUserManagerAppService = customerUserManagerAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
    }

    [BindProperty] public CreateManagerViewModel CreateManager { get; set; }

    public IEnumerable<SelectListItem> CustomersUserProfile { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task OnGetAsync()
    {
        this.CreateManager = new CreateManagerViewModel
        {
            TenantId = this.Id
        };
        this.CustomersUserProfile = (await this._customerUserProfilesAppService.GetCustomerUserProfileListAsync())
            .Where(x => !x.LastName.IsNullOrWhiteSpace())
            .Where(x => x.TenantId == null)
            .OrderBy(x => x.LastName)
            .RenderToSelectList(x => $"{x.LastName} {x.FirstName} {x.MiddleName} ({x.UserName})", x => x.Id.ToString());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Guid.TryParse(this.CreateManager.CustomerUserProfileId, out var customerUserProfileId))
        {
            await this._customerUserManagerAppService.AddManagerByTenantIdAsync(customerUserProfileId,
                this.CreateManager.TenantId);
        }

        return this.NoContent();
    }
}

public class CreateManagerViewModel
{
    [HiddenInput] public Guid TenantId { get; set; }

    [Required] public string CustomerUserProfileId { get; set; }
}
