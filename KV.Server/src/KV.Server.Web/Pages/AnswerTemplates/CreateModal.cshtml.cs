namespace KV.Server.Web.Pages.AnswerTemplates;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class CreateModalModel : ServerPageModel
{
    private readonly ICrudAnswerTemplateService _service;

    public CreateModalModel(ICrudAnswerTemplateService service)
    {
        this._service = service;
    }

    [BindProperty] public CreateAnswerTemplateViewModel AnswerTemplate { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        this.ValidateModel();
        var dto = this.ObjectMapper.Map<CreateAnswerTemplateViewModel, CreateUpdateAnswerTemplateDto>(this.AnswerTemplate);
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

public class CreateAnswerTemplateViewModel
{
    [Required][DataType(DataType.Text)] public string Title { get; set; }

    [Required][DataType(DataType.Text)] public string Description { get; set; }
}
