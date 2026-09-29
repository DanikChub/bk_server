namespace KV.Server.Web.Pages.TicketSections;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class CreateModalModel : ServerPageModel
{
    private readonly ICrudTicketSectionAppService _service;

    public CreateModalModel(ICrudTicketSectionAppService service)
    {
        this._service = service;
    }

    [BindProperty] public CreateTicketSectionViewModel TicketSection { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        this.ValidateModel();
        var dto = this.ObjectMapper.Map<CreateTicketSectionViewModel, CreateUpdateTicketSectionDto>(this.TicketSection);
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

public class CreateTicketSectionViewModel
{
    [Required][DataType(DataType.Text)] public string Name { get; set; }
}
