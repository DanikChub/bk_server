namespace KV.Server.Web.Pages.Customers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class EditCustomerModalModel : ServerPageModel
{
    private readonly ICrudCustomerAppService _crudCustomerAppService;

    public EditCustomerModalModel(ICrudCustomerAppService crudCustomerAppService)
    {
        this._crudCustomerAppService = crudCustomerAppService;
    }

    [BindProperty] public EditCustomerModal CustomerEdit { get; set; }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid Id { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        this.Id = id;
        this.CustomerEdit = new EditCustomerModal();
        var res = await this._crudCustomerAppService.GetAsync(this.Id);
        this.CustomerEdit = this.ObjectMapper.Map<CustomerDto, EditCustomerModal>(res);
    }

    public async Task<IActionResult> OnPost()
    {
        var updateEntity = this.ObjectMapper.Map<EditCustomerModal, CreateUpdateCustomerDto>(this.CustomerEdit);
        try
        {
            await this._crudCustomerAppService.UpdateAsync(this.Id, updateEntity);
        }
        catch (Exception ex)
        {
            return this.BadRequest(ex.Message);
        }

        return this.NoContent();
    }
}

public class EditCustomerModal
{
    [Required][MaxLength(1000)] public string ShortName { get; set; }

    [Required][MaxLength(2000)] public string LongName { get; set; }

    [Required][MaxLength(2000)] public string Address { get; set; }

    [Required] public string DisplayName { get; set; }

    public string SiteUrl { get; set; }
    public string ContractEmail { get; set; }
    public string ContractPhoneNumber { get; set; }
    public string Description { get; set; }
    public string INNNumber { get; set; }
    public string KPPNumber { get; set; }
    public string CommentNotes { get; set; }
    public string ContractOwner { get; set; }
}
