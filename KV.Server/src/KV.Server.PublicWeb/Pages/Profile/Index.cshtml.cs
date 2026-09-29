namespace KV.Server.PublicWeb.Pages.Profile;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

[Authorize]
public class IndexModel : ServerPageModel
{
    private readonly IContractsAppService _contractsAppService;
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;

    public List<CustomerUserProfileDto> Colleagues { get; set; } = new();

    public PagedResultDto<ContractDto> Contracts { get; set; } = new();

    public CustomerUserProfileDto CustomerUserProfile { get; set; } = new();

    public CustomerDto TenantProfile { get; set; } = new();

    public IndexModel(IContractsAppService contractsAppService,
        ICustomerUserProfilesAppService customerUserProfilesAppService,
        ICrudCustomerAppService crudCustomerAppService,
        ITicketHoursSpentHistoryAppService ticketHoursSpentHistoryAppService)
    {
        this._contractsAppService = contractsAppService;
        this._customerUserProfilesAppService = customerUserProfilesAppService;
        this._crudCustomerAppService = crudCustomerAppService;
    }

    public int PrevPage { get; set; }
    public int NextPage { get; set; }

    [BindProperty(Name = "skipPage", SupportsGet = true)]
    public int CurrentPage { get; set; }

    [BindProperty(Name = "name", SupportsGet = true)]
    public string Name { get; set; } = string.Empty;

    public int ContractsCount { get; set; }

    public async Task OnGetAsync()
    {
        this.CustomerUserProfile = await this._customerUserProfilesAppService.GetCustomerByUserNameAsync(this.CurrentUser.UserName);
        this.Colleagues = await this._customerUserProfilesAppService.GetColleaguesByIdentityUserIdAsync(this.CurrentUser.Id);
        this.TenantProfile = await this._crudCustomerAppService.FirstOrDefaultByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty);

        this.ContractsCount = await this._contractsAppService.GetCountContractsByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty);
        var page = new GetContractListRequestDto();
        var maxPage = this.ContractsCount / LimitedResultRequestDto.DefaultMaxResultCount;
        this.PrevPage = this.CurrentPage == 0 ? 0 : this.CurrentPage - 1;
        this.NextPage = this.CurrentPage < maxPage ? this.CurrentPage + 1 : maxPage;
        this.Contracts = await this._contractsAppService.GetContractListByTenantIdAsync(this.CurrentTenant?.Id ?? Guid.Empty, page,
            page.SkipCount, this.ContractsCount, this.Name);

        foreach (var contract in this.Contracts.Items)
        {
            contract.ContractStatistics =
                await this._crudCustomerAppService.GetStatisticsConstraintByContractIdAsync(contract?.Id ?? Guid.Empty);
        }
    }
}
