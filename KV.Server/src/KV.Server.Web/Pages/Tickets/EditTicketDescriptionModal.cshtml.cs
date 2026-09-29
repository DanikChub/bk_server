namespace KV.Server.Web.Pages.Tickets;

using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class EditTicketDescriptionModalModel : ServerPageModel
{
    private readonly ITicketsAppService _ticketAppService;

    public EditTicketDescriptionModalModel(ITicketsAppService ticketAppService)
    {
        this._ticketAppService = ticketAppService;
    }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public long Id { get; set; }

    [BindProperty] public UpdateTicketDescriptionDto UpdateTicketDescriptionDto { get; set; }

    public async Task OnGetAsync(long id)
    {
        this.Id = id;
        var ticket = await this._ticketAppService.GetTicketDetailsAsync(id);
        this.UpdateTicketDescriptionDto = new UpdateTicketDescriptionDto
        {
            Description = ticket.Description
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await this._ticketAppService.UpdateTicketDescriptionAsync(this.Id, this.UpdateTicketDescriptionDto);
        return this.NoContent();
    }
}
