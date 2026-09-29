namespace KV.Server.PublicWeb.Pages.Tickets;
using Microsoft.AspNetCore.Mvc;

public class PreviewTicketPrintModalModel : ServerPageModel
{
    [BindProperty] public long TicketId { get; set; }

    public void OnGet(long ticketId) => this.TicketId = ticketId;
}
