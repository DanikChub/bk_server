namespace KV.Server.Web.Pages.Customers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class EditContractModel : ServerPageModel
{
    public const string NpaLowerCaseName = "нпа";
    public const string PurchaseLowerCaseName = "закупки";
    public const string TradingLowerCaseName = "торги";
    public const string ClaimLowerCaseName = "претензии";
    public const string AccountingLowerCaseName = "бухучет";

    private readonly IConstraintAppService _constraintAppService;
    private readonly IContractsAppService _contractAppService;
    private readonly ICrudContractService _crudContractService;
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly ICrudServicePackageService _crudServicePackageService;

    public EditContractModel(IContractsAppService contractAppService,
        ICrudContractService crudContractService,
        ICrudCustomerAppService crudCustomerAppService,
        ICrudServicePackageService crudServicePackageService,
        IConstraintAppService constraintAppService)
    {
        this._contractAppService = contractAppService;
        this._crudContractService = crudContractService;
        this._crudCustomerAppService = crudCustomerAppService;
        this._crudServicePackageService = crudServicePackageService;
        this._constraintAppService = constraintAppService;
    }

    [BindProperty] public UpdateTenantContractViewModel EditContract { get; set; }

    [BindProperty] public CustomerDto Customer { get; set; }

    public List<SelectListItem> ServicePackages { get; set; }

    public List<CustomerServicePackageDto> ContractServicePackages { get; set; }

    [BindProperty] public List<ContractSettingDto> ContractSettings { get; set; }

    public List<SelectListItem> StandartContractRangeNumbers { get; set; }

    public List<TagDto> Tags { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        Tags = await _crudContractService.GetTagListByContractIdAsync(id);
        var numberConstraintRange = new[] { 10, 0, 5, 15, 20, 25, 30, 35, 40, 45, 50, 70, 75, 80, 100, 120, 150, 200, 500 };
        this.StandartContractRangeNumbers = new List<SelectListItem>();
        foreach (var numberConstraint in numberConstraintRange)
        {
            this.StandartContractRangeNumbers.Add(new SelectListItem
            {
                Value = numberConstraint.ToString(CultureInfo.InvariantCulture),
                Text = numberConstraint.ToString(CultureInfo.InvariantCulture)
            });
        }

        var contract = await this._contractAppService.GetContractAsync(id);
        this.ContractServicePackages =
            await this._crudCustomerAppService.GetActiveServicePackagesByContractIdAsync(contract.Id);
        this.EditContract = this.ObjectMapper.Map<ContractDto, UpdateTenantContractViewModel>(contract);
        var customer = await _crudCustomerAppService.GetAsync((Guid)contract.TenantId);
        EditContract.CustomerName = customer.DisplayName;
        this.EditContract.ServicePackageId = this.ContractServicePackages.FirstOrDefault()?.Id;
        this.Customer = await this._crudCustomerAppService.GetAsync(contract.TenantId ?? Guid.Empty);

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

        this.ContractSettings = await this._constraintAppService.GetConstraintTypeBySettingContractIdAsync(id);
        var npa = this.ContractSettings.FirstOrDefault(x => x.ConstraintTypeName?.ToLowerInvariant() == NpaLowerCaseName);
        var purchase = this.ContractSettings.FirstOrDefault(x => x.ConstraintTypeName?.ToLowerInvariant() == PurchaseLowerCaseName);
        var trading = this.ContractSettings.FirstOrDefault(x => x.ConstraintTypeName?.ToLowerInvariant() == TradingLowerCaseName);
        var accounting = this.ContractSettings.FirstOrDefault(x => x.ConstraintTypeName?.ToLowerInvariant() == AccountingLowerCaseName);
        var claim = this.ContractSettings.FirstOrDefault(x => x.ConstraintTypeName?.ToLowerInvariant() == ClaimLowerCaseName);
        this.EditContract.NPARange = npa?.Max ?? 0;
        this.EditContract.PurchaseRange = purchase?.Max ?? 0;
        this.EditContract.TradingRange = trading?.Max ?? 0;
        this.EditContract.AccountingRange = accounting?.Max ?? 0;
        this.EditContract.ClaimRange = claim?.Max ?? 0;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var customerDto = this.ObjectMapper.Map<UpdateTenantContractViewModel, CreateUpdateContractDto>(this.EditContract);
        await this._crudContractService.UpdateAsync(this.EditContract.Id, customerDto);
        await this._crudContractService.UpdateDetailsAsync(this.EditContract.Id, new CreateUpdateDetailsContractDto
        {
            ServicePackageId = this.EditContract.ServicePackageId
        });
        var constraintTypes = await this._constraintAppService.GetConstraintTypesAsync();
        var npa = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == NpaLowerCaseName);
        var purchase = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == PurchaseLowerCaseName);
        var trading = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == TradingLowerCaseName);
        var accounting = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == AccountingLowerCaseName);
        var claim = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == ClaimLowerCaseName);
        await this._crudContractService.UpdateMaxConstraintAsync(this.EditContract.Id, npa?.Id ?? Guid.Empty, this.EditContract.NPARange);
        await this._crudContractService.UpdateMaxConstraintAsync(this.EditContract.Id, purchase?.Id ?? Guid.Empty, this.EditContract.PurchaseRange);
        await this._crudContractService.UpdateMaxConstraintAsync(this.EditContract.Id, trading?.Id ?? Guid.Empty, this.EditContract.TradingRange);
        await this._crudContractService.UpdateMaxConstraintAsync(this.EditContract.Id, accounting?.Id ?? Guid.Empty, this.EditContract.AccountingRange);
        await this._crudContractService.UpdateMaxConstraintAsync(this.EditContract.Id, claim?.Id ?? Guid.Empty, this.EditContract.ClaimRange);
        return this.NoContent();
    }
}

public class UpdateTenantContractViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime ContractStartDate { get; set; }
    public DateTime ContractFinishDate { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? ServicePackageId { get; set; }
    public string ContractPayer { get; set; }
    public int NPARange { get; set; }
    public int PurchaseRange { get; set; }
    public int TradingRange { get; set; }
    public int AccountingRange { get; set; }

    public string CustomerName { get; set; }
    public int ClaimRange { get; set; }
    public List<ConstraintTypeContractDto> ConstraintTypeContracts { get; set; }
}
