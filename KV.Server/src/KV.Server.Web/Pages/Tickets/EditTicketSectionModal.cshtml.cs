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
public class EditTicketSectionModal : ServerPageModel
{
    private readonly ITicketsAppService _ticketsAppService;
    private readonly ITicketSectionAppService _ticketSectionAppService;

    public EditTicketSectionModal(ITicketSectionAppService ticketSectionAppService,
        ITicketsAppService ticketsAppService)
    {
        this._ticketSectionAppService = ticketSectionAppService;
        this._ticketsAppService = ticketsAppService;
    }

    [BindProperty] public EditTicketSectionViewModel TicketSection { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public long TicketId { get; set; }

    public List<SelectListItem> TicketSections { get; set; }

    public async Task OnGetAsync()
    {
        this.TicketSection = new EditTicketSectionViewModel
        {
            TicketId = this.TicketId
        };
        this.TicketSections = (await this._ticketSectionAppService.GetTicketSectionsAsync())
            .RenderToSelectList(x => x.Name, x => x.Id)
            .ToList();

        this.TicketSections.AddFirst(new SelectListItem
        {
            Text = "Отсутствует",
            Selected = true,
            Value = ""
        });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (this.TicketSection?.TicketSectionId != null || this.TicketSection?.TicketSectionId == Guid.Empty)
        {
            await this._ticketsAppService.UpdateTicketSectionByTicketIdAsync(this.TicketSection.TicketId,
                this.TicketSection?.TicketSectionId ?? Guid.Empty);
        }

        return this.NoContent();
    }

    public class EditTicketSectionViewModel
    {
        public Guid? TicketSectionId { get; set; }

        [HiddenInput] public long TicketId { get; set; }
    }
}
