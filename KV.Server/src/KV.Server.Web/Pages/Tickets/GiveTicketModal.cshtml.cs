namespace KV.Server.Web.Pages.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class GiveTicketModalModel : ServerPageModel
{
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly ITicketsAppService _ticketsAppService;

    public GiveTicketModalModel(ICustomerUserProfilesAppService customerUserProfilesAppService,
        ITicketsAppService ticketsAppService)
    {
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._ticketsAppService = ticketsAppService;
    }

    [BindProperty] public GiveTicketViewModel GiveTicket { get; set; }

    public IEnumerable<SelectListItem> Employees { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public long TicketId { get; set; }

    public async Task OnGetAsync()
    {
        this.GiveTicket = new GiveTicketViewModel
        {
            CustomerUserProfileId = (await this._ticketsAppService.GetTicketDetailsAsync(this.TicketId))?.ResponsibleId.ToString()
        };
        var specialists = await this._customerUserProfilesAppService.GetSpecialistsAsync();
        this.Employees = specialists
            .OrderBy(x => x.LastName)
            .Where(x => !(x.LastName.IsNullOrWhiteSpace() && x.FirstName.IsNullOrWhiteSpace() &&
                          x.MiddleName.IsNullOrWhiteSpace()))
            .RenderToSelectList(x => $"{x.LastName} {x.FirstName} {x.MiddleName}", x => x.Id.ToString());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Guid.TryParse(this.GiveTicket.CustomerUserProfileId, out var customerUserProfileId))
        {
            await this._ticketsAppService.AssignedSpecialistAsync(this.TicketId, customerUserProfileId);
        }

        return this.NoContent();
    }

    public class GiveTicketViewModel
    {
        [Required] public string CustomerUserProfileId { get; set; }
    }
}
