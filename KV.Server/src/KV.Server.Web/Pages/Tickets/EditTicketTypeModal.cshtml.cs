namespace KV.Server.Web.Pages.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class EditTicketTypeModal : ServerPageModel
{
    private readonly ITicketsAppService _ticketsAppService;
    private readonly ITicketTypeAppService _ticketTypeAppService;

    public EditTicketTypeModal(ITicketTypeAppService ticketTypeAppService,
        ITicketsAppService ticketsAppService)
    {
        this._ticketTypeAppService = ticketTypeAppService;
        this._ticketsAppService = ticketsAppService;
    }

    [BindProperty] public CreateTicketTypeViewModel TicketType { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public long TicketId { get; set; }

    public IEnumerable<SelectListItem> TicketTypes { get; set; }

    public async Task OnGetAsync()
    {
        this.TicketType = new CreateTicketTypeViewModel
        {
            TicketId = this.TicketId
        };
        this.TicketTypes = (await this._ticketTypeAppService.GetTicketTypesAsync())
            .RenderToSelectList(x => x.Name, x => x.Id);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await this._ticketsAppService.UpdateTicketTypeByTicketIdAsync(this.TicketType.TicketId, this.TicketType.TicketTypeId);
        return this.NoContent();
    }

    public class CreateTicketTypeViewModel
    {
        public Guid TicketTypeId { get; set; }

        [HiddenInput] public long TicketId { get; set; }
    }
}
