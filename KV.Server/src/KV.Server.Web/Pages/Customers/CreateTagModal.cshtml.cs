using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KV.Server.Web.Pages.Customers;

public class CreateTagModalModel : PageModel
{
    private readonly ICrudContractService _contractService;

    public CreateTagModalModel(ICrudContractService contractService)
    {
        _contractService = contractService;
    }
    [BindProperty]
    public CreateTag ContractCreateTag { get; set; }

    public string TagName { get; set; }
    public async Task OnGetAsync(Guid id)
    {
        ContractCreateTag = new CreateTag
        {
            ContractId = id,
        };
    }
    public async Task OnPostAsync()
    {
        await _contractService.CreateTagByContractIdAsync(ContractCreateTag.ContractId, ContractCreateTag.TagName);
    }
}
public class CreateTag
{
    public string TagName { get; set; }
    [HiddenInput]
    public Guid ContractId { get; set; }
}
