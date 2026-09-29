namespace Server.Web.Pages.Tickets;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using KV.Server.Web.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class EditModalModel : ServerPageModel
{
    private readonly ITicketsAppService _ticketsAppService;
    private readonly ITicketTypeAppService _ticketTypeAppService;

    public EditModalModel(ITicketsAppService ticketsAppService,
        ITicketTypeAppService ticketTypeAppService)
    {
        this._ticketsAppService = ticketsAppService;
        this._ticketTypeAppService = ticketTypeAppService;
    }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public long Id { get; set; }

    [BindProperty] public CreateUpdateTicketDto Ticket { get; set; }

    public IEnumerable<SelectListItem> TicketTypesList { get; set; }

    public async Task OnGetAsync(long id)
    {
        var ticketDto = await this._ticketsAppService.GetTicketDetailsAsync(id);
        this.Id = id;
        this.Ticket = this.ObjectMapper.Map<TicketDetailsDto, CreateUpdateTicketDto>(ticketDto);

        this.TicketTypesList = (await this._ticketTypeAppService.GetListAsync(new GetTicketTypeListRequestDto()))
            .Items
            .RenderToSelectList(x => x.Name, x => x.Id);
    }

    public IActionResult OnPost()
    {
        this.ValidateModel();
        //await _ticketsAppService.UpdateTicketAsync();
        return this.NoContent();
    }
}
