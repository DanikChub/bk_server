namespace KV.Server.Web.Pages.Customers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class CreateEventModalModel : ServerPageModel
{
    private readonly IEventAppService _eventAppService;

    public CreateEventModalModel(IEventAppService eventAppService)
    {
        this._eventAppService = eventAppService;
    }

    [BindProperty] public CreateEventVM CreateEventVM { get; set; }

    public void OnGet(Guid tenantId)
    {
        this.CreateEventVM = new CreateEventVM
        {
            TenantId = tenantId,
            EventDate = this.Clock.Now.Date
        };
    }
    public async Task OnPostAsync()
    {
        var dto = this.ObjectMapper.Map<CreateEventVM, CreateEventInTicketDto>(this.CreateEventVM);
        await this._eventAppService.CreateEventAsync(dto);
    }
}
public class CreateEventVM
{
    [HiddenInput]
    public Guid TenantId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime EventDate { get; set; }
}
