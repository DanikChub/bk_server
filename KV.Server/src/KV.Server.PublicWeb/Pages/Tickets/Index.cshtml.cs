namespace KV.Server.PublicWeb.Pages.Tickets;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class IndexModel : ServerPageModel
{
    public const string TicketStatusDraftLower = "draft";
    private readonly ICrudServicePackageService _crudServicePackageService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;
    private readonly ITicketStatusAppService _ticketStatusAppService;

    public TicketStatusInfoDto FavoriteTicket { get; set; } = new();

    public TicketStatusInfoDto MyTicket { get; set; } = new();

    public int TicketsCount { get; set; }

    public List<ServicePackageDto> TicketServicePackagesView { get; set; } = new();

    public List<TicketStatusInfoDto> TicketStatusesView { get; set; } = new();

    public IndexModel(ITicketStatusAppService ticketStatusAppService,
        ICrudServicePackageService crudServicePackageService,
        ICustomerUserProfilesAppService customerUserProfilesAppService)
    {
        this._ticketStatusAppService = ticketStatusAppService;
        this._crudServicePackageService = crudServicePackageService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
    }

    public int PrevPage { get; set; }
    public int NextPage { get; set; }

    [BindProperty(Name = "skipPage", SupportsGet = true)]
    public int CurrentPage { get; set; }

    [BindProperty(Name = "ticketStatusId", SupportsGet = true)]
    public string? TicketStatusId { get; set; }

    [BindProperty(Name = "servicepackageid", SupportsGet = true)]
    public Guid? ServicePackageId { get; set; }

    [BindProperty(Name = "subject", SupportsGet = true)]
    public string? Subject { get; set; }

    public bool IsDraftPage { get; set; }

    public async Task OnGetAsync()
    {
        this.TicketStatusesView = await this._ticketStatusAppService.GetTicketStatusByCreatorIdAndTenantAsync(this.CurrentUser?.Id);
        this.TicketStatusesView.Remove(TicketStatusesView.Where(x => x.Name == TicketStatusConstants.Renewed).FirstOrDefault());

        this.TicketServicePackagesView = await this._crudServicePackageService.ToListAsync();
        var draftTicketStatusId = this.TicketStatusesView.FirstOrDefault(
            x => x.Name.ToLowerInvariant() == TicketStatusDraftLower)?.Id
            .ToString();
        if (this.TicketStatusId.IsNullOrWhiteSpace())
        {
            this.TicketStatusId = this.TicketStatusesView.FirstOrDefault()?.Id.ToString();
        }

        this.IsDraftPage = draftTicketStatusId == this.TicketStatusId;
        var currentCustomerUserProfile =
            await this._customerUserProfilesAppService.GetCustomerByIdentityUserIdAsync(this.CurrentUser?.Id ?? Guid.Empty);
        this.MyTicket = await this._ticketStatusAppService.GetMyTicketStatusByResponsibleIdAsync(currentCustomerUserProfile?.Id ??
            Guid.Empty);
        this.FavoriteTicket = await this._ticketStatusAppService.GetTicketFavoriteInfoAsync();
        if (this.TicketStatusId == this.MyTicket.Name)
        {
            this.TicketsCount = (int)this.MyTicket.Count;
        }
        else if (this.TicketStatusId == this.FavoriteTicket.Name)
        {
            this.TicketsCount = (int)this.FavoriteTicket.Count;
        }
        else
        {
            this.TicketsCount =
                (int)await this._ticketStatusAppService.GetCountTicketByTicketStatusAsync(this.TicketStatusId?.To<Guid>());
        }
    }
}
