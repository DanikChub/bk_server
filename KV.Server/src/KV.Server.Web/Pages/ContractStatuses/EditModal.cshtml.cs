namespace KV.Server.Web.Pages.ContractStatuses;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class EditModalModel : ServerPageModel
{
    private readonly ICrudContractStatusService _service;

    public EditModalModel(ICrudContractStatusService service)
    {
        this._service = service;
    }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty] public UpdateContractStatusViewModel ContractStatus { get; set; }

    public async Task OnGetAsync()
    {
        var dto = await this._service.GetAsync(this.Id);
        this.ContractStatus = this.ObjectMapper.Map<ContractStatusDto, UpdateContractStatusViewModel>(dto);
    }

    public async Task OnPostAsync()
    {
        this.ValidateModel();

        var dto = this.ObjectMapper.Map<UpdateContractStatusViewModel, CreateUpdateContractStatusDto>(this.ContractStatus);
        dto.TenantId = this.CurrentTenant.Id;
        await this._service.UpdateAsync(this.Id, dto);
    }
}

public class UpdateContractStatusViewModel
{
    [HiddenInput] public Guid TenantId { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(20)]
    public string Title { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(20)]
    public string Code { get; set; }
}
