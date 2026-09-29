namespace KV.Server.Web.Pages.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using KV.Server.Tags;
using KV.Server.Web.Pages.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class CreateContractModalModel : ServerPageModel
{
    private readonly ICrudContractService _contractCrudService;
    private readonly ICrudContractStatusService _contractStatusService;
    private readonly ICRUDTenantProfileAppService _crudTenantProfileAppService;
    private readonly ITagsAppService _tagsAppService;
    private readonly IContractsAppService _contractsAppService;
    private readonly IConstraintAppService _constraintAppService;


    public CreateContractModalModel(ICrudContractService contractCrudService,
        ICrudContractStatusService contractStatusService,
        ICRUDTenantProfileAppService crudTenantProfileAppService,
        ITagsAppService tagsAppService,
        IContractsAppService contractsAppService,
        IConstraintAppService constraintAppService)
    {
        this._contractCrudService = contractCrudService;
        this._contractStatusService = contractStatusService;
        this._crudTenantProfileAppService = crudTenantProfileAppService;
        _tagsAppService = tagsAppService;
        _contractsAppService = contractsAppService;
        _constraintAppService = constraintAppService;
    }

    [BindProperty] public CreateContactViewModel Contract { get; set; }

    public IEnumerable<SelectListItem> StatusList { get; set; }

    public IEnumerable<SelectListItem> Clients { get; set; }
    public List<SelectListItem> ServicePacks { get; set; }
    public List<TagDto> SelectedTags { get; set; }
    public async Task OnGetAsync()
    {
         ServicePacks = new List<SelectListItem>();
        SelectedTags = (await _tagsAppService.GetPopularByTicketTagAsync()).DistinctBy(x => x.Name).ToList();
        this.Contract = new CreateContactViewModel();
        var servicePackages = await _contractsAppService.GetListServicePackagesAsync();
        foreach (var servicePackage in servicePackages)
        {
            ServicePacks.Add(new SelectListItem { Text = servicePackage.Name, Value = servicePackage.Id.ToString() });
        }
        var statuses = await this._contractStatusService.GetContractStatusesListAsync();
        var status = statuses.FirstOrDefault(x => x.Code.ToLowerInvariant() == "in progress");
        if (status != null)
        {
            this.Contract.StatusId = status.Id;
        }

        this.StatusList = new List<SelectListItem>();
        var clients = await this._crudTenantProfileAppService.GetTenantProfileListAsync();

        this.Clients = clients
            .OrderBy(x => x.ShortName)
            .RenderToSelectList(x => x.ShortName, x => x.Id);
    }


    public async Task<IActionResult> OnPostAsync()
    {
        var requestData = this.ObjectMapper.Map<CreateContactViewModel, CreateUpdateContractDto>(this.Contract);
        requestData.TenantId = this.Contract.TenantId;
        var status = (await this._contractStatusService.GetAsync(this.Contract.StatusId));
        requestData.StatusId = status.Id;
        requestData.StatusCode = status.Code;
        requestData.ServicePackageId = this.Contract.ServicePackageId;
        var contract = await this._contractCrudService.CreateAsync(requestData);
        await _contractsAppService.CreateContractServicePackageAsync(contract.Id, Contract.ServicePackageId, contract.ContractStartDate, contract.ContractFinishDate);
        if (Contract.TagNames != null)
        {
            foreach (var item in this.Contract.TagNames)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    await _contractCrudService.CreateTagByContractIdAsync(contract.Id, item);
                }
            }
        }
        var constraintTypes = await this._constraintAppService.GetConstraintTypesAsync();
        var npa = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == EditContractModel.NpaLowerCaseName);
        var purchase = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == EditContractModel.PurchaseLowerCaseName);
        var trading = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == EditContractModel.TradingLowerCaseName);
        var accounting = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == EditContractModel.AccountingLowerCaseName);
        var claim = constraintTypes.FirstOrDefault(x => x.Name.ToLowerInvariant() == EditContractModel.ClaimLowerCaseName);
        if (Contract.NPARange != 0)
        {
            await this._contractCrudService.UpdateMaxConstraintAsync(contract.Id, npa.Id, Contract.NPARange);
        }

        if (Contract.PurchaseRange != 0)
        {
            await this._contractCrudService.UpdateMaxConstraintAsync(contract.Id, purchase.Id, this.Contract.PurchaseRange);
        }

        if (Contract.TradingRange != 0)
        {
            await this._contractCrudService.UpdateMaxConstraintAsync(contract.Id, trading.Id, this.Contract.TradingRange);
        }

        if (Contract.AccountingRange != 0)
        {
            await this._contractCrudService.UpdateMaxConstraintAsync(contract.Id, accounting.Id, this.Contract.AccountingRange);
        }

        if (Contract.ClaimRange != 0)
        {
            await this._contractCrudService.UpdateMaxConstraintAsync(contract.Id, claim.Id, this.Contract.ClaimRange);
        }
        var json = new JsonResult(new { contract.Id });
        return json;
    }
}

public class CreateContactViewModel
{
    public List<string> TagNames { get; set; }
    public Guid ServicePackageId { get; set; }
    public int NPARange { get; set; }
    public int PurchaseRange { get; set; }
    public int TradingRange { get; set; }
    public int AccountingRange { get; set; }
    public int ClaimRange { get; set; }
    [Required][DataType(DataType.Text)] public string Name { get; set; }

    [Required][DataType(DataType.Date)] public DateTime ContractStartDate { get; set; } = DateTime.Now.Date;

    [Required]
    [DataType(DataType.Date)]
    public DateTime ContractFinishDate { get; set; } = DateTime.Now.Date.AddYears(1);
    [HiddenInput] public string StatusCode { get; set; }
    [Required] public Guid TenantId { get; set; }

    [Required] public Guid StatusId { get; set; }
}
