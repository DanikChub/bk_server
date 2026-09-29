namespace KV.Server.Web.Pages.Customers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class CreateServicePackageModalModel : ServerPageModel
{
    private readonly IContractsAppService _contractAppService;
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly ICrudServicePackageService _crudServicePackageService;
    private readonly ICrudContractService _crudContractService;

    public CreateServicePackageModalModel(IContractsAppService contractAppService, ICrudCustomerAppService crudCustomerAppService, ICrudServicePackageService crudServicePackageService, ICrudContractService crudContractService)
    {
        this._contractAppService = contractAppService;
        this._crudCustomerAppService = crudCustomerAppService;
        this._crudServicePackageService = crudServicePackageService;
        this._crudContractService = crudContractService;
    }
    public List<SelectListItem> ServicePackages { get; set; }
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    [BindProperty(SupportsGet = true)]
    public Guid ServiceId { get; set; }
    public async Task OnGetAsync(Guid id)
    {
        this.Id = id;
        this.ServicePackages = new List<SelectListItem>();
        var servicePackages = await this._crudServicePackageService.ToListAsync();
        foreach (var servicePackage in servicePackages)
        {
            this.ServicePackages.Add(new SelectListItem
            {
                Text = servicePackage.Name,
                Value = servicePackage.Id.ToString()
            });
        }
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var contract = await this._contractAppService.GetContractAsync(this.Id);

        await this._contractAppService.CreateContractServicePackageAsync(this.Id, this.ServiceId, contract.ContractStartDate, contract.ContractFinishDate);

        return this.NoContent();
    }
}
