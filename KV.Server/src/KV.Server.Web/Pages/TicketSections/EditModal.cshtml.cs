namespace KV.Server.Web.Pages.TicketSections;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class EditModalModel : ServerPageModel
{
    private readonly ICrudTicketSectionAppService _service;

    public EditModalModel(ICrudTicketSectionAppService service)
    {
        this._service = service;
    }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty] public UpdateTicketSectionViewModel TicketSection { get; set; }

    public async Task OnGetAsync()
    {
        var dto = await this._service.GetAsync(this.Id);
        this.TicketSection = this.ObjectMapper.Map<TicketSectionDto, UpdateTicketSectionViewModel>(dto);
    }

    public async Task OnPostAsync()
    {
        this.ValidateModel();
        var dto = this.ObjectMapper.Map<UpdateTicketSectionViewModel, CreateUpdateTicketSectionDto>(this.TicketSection);
        await this._service.UpdateAsync(this.Id, dto);
    }
}

public class UpdateTicketSectionViewModel
{
    [Required][DataType(DataType.Text)] public string Name { get; set; }
}
