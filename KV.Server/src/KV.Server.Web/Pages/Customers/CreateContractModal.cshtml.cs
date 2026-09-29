using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.MultiTenancy;

namespace KV.Server.Web.Pages.Customers;


public class CreateContractModalModel : ServerPageModel
{
    private readonly ICrudContractService _contractCrudService;
    private readonly IConstraintAppService _constraintAppService;
    private readonly ICrudContractStatusService _contractStatusService;
    private readonly IContractsAppService _contractsAppService;
    private readonly ITagsAppService _tagsAppService;
    private readonly ICRUDTenantProfileAppService _crudTenantProfileAppService;
    private readonly ICrudServicePackageService _crudServicePackageService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;

    public CreateContractModalModel(ICrudContractService contractCrudService,
        ICrudContractStatusService contractStatusService,
        ICRUDTenantProfileAppService crudTenantProfileAppService,
        ICurrentTenant currentTenant,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        IContractsAppService contractsAppService,
        ICrudServicePackageService crudServicePackageService,
        IConstraintAppService constraintAppService,
        ITagsAppService tagsAppService)
    {
        this._contractCrudService = contractCrudService;
        this._contractStatusService = contractStatusService;
        this._crudTenantProfileAppService = crudTenantProfileAppService;
        _currentTenant = currentTenant;
        _customerUserProfilesAppService = customerUserProfilesAppService;
        _contractsAppService = contractsAppService;
        _crudServicePackageService = crudServicePackageService;
        _constraintAppService = constraintAppService;
        _tagsAppService = tagsAppService;
    }

    [BindProperty] public CreateContactVM Contract { get; set; }
    public List<CustomerServicePackageDto> ServicePackages { get; set; }
    public IEnumerable<SelectListItem> StatusList { get; set; }
    public List<SelectListItem> ServicePacks { get; set; }
    public List<SelectListItem> Clients { get; set; }
    public List<TagDto> Tags { get; set; }
    public List<TagDto> SelectedTags { get; set; }
    public async Task OnGetAsync(Guid id)
    {

        SelectedTags = (await _tagsAppService.GetPopularByTicketTagAsync()).DistinctBy(x => x.Name).ToList();
        //Tags = await _tagsAppService.GetPopularByTicketTagAsync();
        ServicePacks = new List<SelectListItem>();
        var servicePackages = await _contractsAppService.GetListServicePackagesAsync();
        foreach (var servicePackage in servicePackages)
        {
            ServicePacks.Add(new SelectListItem { Text = servicePackage.Name, Value = servicePackage.Id.ToString() });
        }
        this.Contract = new CreateContactVM();
        var statuses = (await this._contractStatusService.GetContractStatusesListAsync()).Where(x => x.TenantId == id);
        var status = statuses.FirstOrDefault(x => x.Code.ToLowerInvariant() == "in progress");
        if (status != null)
        {
            this.Contract.StatusId = status.Id;
        }
        Clients = new List<SelectListItem>();
        this.StatusList = statuses.RenderToSelectList(x => x.Title, x => x.Id);
        var customer = await _crudTenantProfileAppService.GetAsync(id);
        Contract.TenantId = customer.Id;
        Contract.StatusCode = status.Code;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var requestData = this.ObjectMapper.Map<CreateContactVM, CreateUpdateContractDto>(this.Contract);
        requestData.TenantId = this.Contract.TenantId;
        var status = (await this._contractStatusService.GetContractStatusesListAsync()).Where(x => x.Code == Contract.StatusCode).FirstOrDefault();
        requestData.StatusCode = status.Code;
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
public class CreateContactVM
{
    public List<string> TagNames { get; set; }
    public Guid ServicePackageId { get; set; }
    [Required][DataType(DataType.Text)] public string Name { get; set; }

    [Required][DataType(DataType.Date)] public DateTime ContractStartDate { get; set; } = DateTime.Now.Date;

    [Required]
    [DataType(DataType.Date)]
    public DateTime ContractFinishDate { get; set; } = DateTime.Now.Date.AddYears(1);

    [Required][HiddenInput] public Guid TenantId { get; set; }

    [HiddenInput] public string StatusCode { get; set; }

    [Required] public Guid StatusId { get; set; }
    public int NPARange { get; set; }
    public int PurchaseRange { get; set; }
    public int TradingRange { get; set; }
    public int AccountingRange { get; set; }
    public int ClaimRange { get; set; }
}
