namespace KV.Server.Web.Pages.Tickets;
using System;
using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class EditTicketHistoryModalModel : ServerPageModel
{
    private readonly ITicketHistoryAppService _ticketHistoryAppService;

    public EditTicketHistoryModalModel(ITicketHistoryAppService ticketHistoryAppService)
    {
        this._ticketHistoryAppService = ticketHistoryAppService;
    }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid Id { get; set; }

    [BindProperty] public EditTicketHistoryViewModel UpdateTicketHistory { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        this.Id = id;
        var ticketHistory = await this._ticketHistoryAppService.GetTicketHistoryAsync(id);
        this.UpdateTicketHistory = this.ObjectMapper.Map<TicketHistoryDto, EditTicketHistoryViewModel>(ticketHistory);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await this._ticketHistoryAppService.UpdateTicketHistoryDescriptionAsync(new UpdateTicketHistoryDescriptionDto
        {
            Id = this.Id,
            Description = this.UpdateTicketHistory.Description
        });
        return this.NoContent();
    }
}

public class EditTicketHistoryViewModel
{
    [HiddenInput] public Guid Id { get; set; }

    public string Description { get; set; }
}
