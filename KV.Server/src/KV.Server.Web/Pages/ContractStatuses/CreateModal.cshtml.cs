namespace KV.Server.Web.Pages.ContractStatuses;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class CreateModalModel : ServerPageModel
{
    private readonly ICrudContractStatusService _service;

    public CreateModalModel(ICrudContractStatusService service)
    {
        this._service = service;
    }

    [BindProperty] public CreateContractStatusViewModel ContractStatus { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        this.ValidateModel();
        var dto = this.ObjectMapper.Map<CreateContractStatusViewModel, CreateUpdateContractStatusDto>(this.ContractStatus);
        dto.TenantId = this.CurrentTenant.Id;
        try
        {
            var result = await this._service.CreateAsync(dto);
        }
        catch
        {
            return this.BadRequest();
        }

        return this.NoContent();
    }
}

public class CreateContractStatusViewModel
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
