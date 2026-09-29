namespace KV.Server.Web.Pages.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class EditContractModalModel : ServerPageModel
{
    private readonly ICrudContractService _contractCrudService;
    private readonly ICrudContractStatusService _contractStatusService;
    private readonly ICRUDTenantProfileAppService _crudTenantProfileAppService;

    public EditContractModalModel(ICrudContractService contractCrudService,
        ICrudContractStatusService contractStatusService,
        ICRUDTenantProfileAppService crudTenantProfileAppService)
    {
        this._contractCrudService = contractCrudService;
        this._contractStatusService = contractStatusService;
        this._crudTenantProfileAppService = crudTenantProfileAppService;
    }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid Id { get; set; }

    [BindProperty] public UpdateContractViewModel Contract { get; set; }

    public IEnumerable<SelectListItem> StatusList { get; set; }

    public IEnumerable<SelectListItem> Clients { get; set; }

    public async Task OnGetAsync()
    {
        var dto = await this._contractCrudService.GetAsync(this.Id);
        this.Contract = this.ObjectMapper.Map<ContractDto, UpdateContractViewModel>(dto);
        this.StatusList = (await this._contractStatusService.GetContractStatusesListAsync())
            .RenderToSelectList(x => x.Title, x => x.Id);
        this.Clients = (await this._crudTenantProfileAppService.GetTenantProfileListAsync())
            .RenderToSelectList(x => x.ShortName, x => x.Id);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        this.ValidateModel();

        var updateEntity = this.ObjectMapper.Map<UpdateContractViewModel, CreateUpdateContractDto>(this.Contract);
        var status = await this._contractStatusService.GetAsync(this.Contract.StatusId);
        updateEntity.StatusId = status.Id;
        try
        {
            await this._contractCrudService.UpdateAsync(this.Id, updateEntity);
        }
        catch (Exception ex)
        {
            return this.BadRequest(ex.Message);
        }

        return this.NoContent();
    }
}

public class UpdateContractViewModel
{
    [Required][DataType(DataType.Text)] public string Name { get; set; }

    [Required][DataType(DataType.Date)] public DateTime ContractStartDate { get; set; }

    [Required][DataType(DataType.Date)] public DateTime ContractFinishDate { get; set; }

    [Required] public Guid StatusId { get; set; }

    [Required][HiddenInput] public Guid TenantId { get; set; }
}
