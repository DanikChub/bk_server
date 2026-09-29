namespace KV.Server.Web.Pages.AnswerTemplates;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class EditModalModel : ServerPageModel
{
    private readonly ICrudAnswerTemplateService _service;

    public EditModalModel(ICrudAnswerTemplateService service)
    {
        this._service = service;
    }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty] public UpdateAnswerTemplateViewModel AnswerTemplate { get; set; }

    public async Task OnGetAsync()
    {
        var dto = await this._service.GetAsync(this.Id);
        this.AnswerTemplate = this.ObjectMapper.Map<AnswerTemplateDto, UpdateAnswerTemplateViewModel>(dto);
    }

    public async Task OnPostAsync()
    {
        this.ValidateModel();
        var dto = this.ObjectMapper.Map<UpdateAnswerTemplateViewModel, CreateUpdateAnswerTemplateDto>(this.AnswerTemplate);
        await this._service.UpdateAsync(this.Id, dto);
    }
}

public class UpdateAnswerTemplateViewModel
{
    [Required][DataType(DataType.Text)] public string Title { get; set; }

    [Required][DataType(DataType.Text)] public string Description { get; set; }
}
