namespace KV.Server.Web.Pages.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class IndexModel : ServerPageModel
{
    public const int MaxCountResponsibleTake = 27;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly ITicketsAppService _ticketsAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;
    private readonly ITableAppService _tableAppService;

    public IndexModel(ITicketStatusAppService ticketStatusAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        ITicketsAppService ticketsAppService,
        ITableAppService tableAppService)
    {
        this._ticketStatusAppService = ticketStatusAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._ticketsAppService = ticketsAppService;
        this._tableAppService = tableAppService;
    }

    [BindProperty(Name = "status", SupportsGet = true)]
    public string TicketStatusId { get; set; }

    public List<TicketResponsibleCountDto> ResponsibleWithMoreCounts { get; set; }

    public List<TicketResponsibleCountDto> ResponsibleWithNoMoreCounts { get; set; }

    public List<SelectListItem> TicketStatuses { get; set; }

    public List<TicketStatusDto> TicketDisplayStatuses { get; set; }
    [BindProperty] public int CountTicket { get; set; }

    [BindProperty(SupportsGet = true)]
    public SearchViewModel Search { get; set; }

    public async Task OnGetAsync()
    {


        this.Search = new SearchViewModel
        {
            TicketStatusId = this.TicketStatusId
        };

        var responsibleCountDtos =
            await this._ticketsAppService.GetResponsibleWithTicketCountByPeriodAsync(Clock.Now.AddDays(-30),
                Clock.Now);
        this.ResponsibleWithMoreCounts = responsibleCountDtos
            .Where(x => x.CountTicketTake > 0)
            .OrderByDescending(x => x.CountTicketTake)
            .Take(MaxCountResponsibleTake)
            .ToList();

        this.TicketStatuses = new List<SelectListItem>
        {
            new SelectListItem { Text = string.Empty, Value = string.Empty }
        };
        var ticketStatuses = await this._ticketStatusAppService.GetListStatusesAsync();
        this.TicketDisplayStatuses = ticketStatuses
            .Where(x => x.IsPublic)
            .ToList();
        foreach (var ticketStatus in ticketStatuses)
        {
            this.TicketStatuses.Add(new SelectListItem
            { Text = ticketStatus.DisplayName, Value = ticketStatus.Id.ToString() });
        }

        if (this.CurrentUser != null)
        {
            var customerUserProfile =
                await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser.Id ?? Guid.Empty);
            if (this.CurrentUser.IsInRole("Менеджер Отдела Продаж"))
            {
                this.Search.ManagerId = customerUserProfile.Id;
            }
        }
    }

    public async Task<IActionResult> OnGetExcelTableAsync()
    {
        var input = this.ObjectMapper.Map<SearchViewModel, GetTicketsListRequestDto>(this.Search);
        input.MaxResultCount = 100;
        var remoteStream = await this._tableAppService.GenerateTicketTableAsync(input);
        return this.File(remoteStream.GetStream(), remoteStream.ContentType, remoteStream.FileName);
    }

    public class SearchViewModel
    {
        public long? TicketId { get; set; }
        public string Subject { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? DueDate { get; set; }
        public string CustomerShortName { get; set; }
        public string CustomerLongName { get; set; }
        public string CreatorFullName { get; set; }
        public string ResponsibleFullName { get; set; }
        public string TicketStatusId { get; set; }
        public string UpperTicketStatusId { get; set; }
        public string TicketTypeId { get; set; }
        public string TicketSectionName { get; set; }
        public string SearchStr { get; set; }

        [HiddenInput] public Guid? ManagerId { get; set; }
    }
}
