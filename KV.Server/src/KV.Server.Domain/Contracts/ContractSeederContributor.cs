namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Tickets;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
public static class TicketStatusConstants
{
    public const string New = "New";
    public const string InProgress = "InProgress";
    public const string WaitingForSpecialistResponse = "WaitingForSpecialistResponse";
    public const string MoreDetails = "MoreDetails";
    public const string Draft = "Draft";
    public const string Answered = "Answered";
    public const string Renewed = "Renewed";
}
public class ContractSeederContributor : ITransientDependency, IDataSeedContributor
{
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IRepository<ContractStatus> _contractStatusRepository;
    private readonly IRepository<TenantProfile> _profileRepository;
    private IRepository<TicketStatus, Guid> _ticketStatusRepository;

    public ContractSeederContributor(IRepository<Contract, Guid> contractRepository,
        IRepository<TenantProfile> profileRepository,
        IRepository<ContractStatus> contractStatusRepository,
        IRepository<TicketStatus, Guid> ticketStatusRepository)
    {
        this._contractRepository = contractRepository;
        this._profileRepository = profileRepository;
        this._contractStatusRepository = contractStatusRepository;
        _ticketStatusRepository = ticketStatusRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        Debug.WriteLine(nameof(SeedAsync));
        await CreateTicketStatusAsync();
        await Task.CompletedTask;
    }

    private async Task CreateTicketStatusAsync()
    {
        var ticketStatuses = await _ticketStatusRepository.WithDetailsAsync();

        await CheckAndInsertTicketStatus(ticketStatuses, TicketStatusConstants.Renewed, "Возобновленные", "Возобновленная", "fa-inbox", null);
        await CheckAndInsertTicketStatus(ticketStatuses, TicketStatusConstants.New, "Новые", "Новая", "fa-newspaper", "label-info");
        await CheckAndInsertTicketStatus(ticketStatuses, TicketStatusConstants.InProgress, "В работе", "В работе", "fa fa-envelope-o", "label-warning");
        await CheckAndInsertTicketStatus(ticketStatuses, TicketStatusConstants.WaitingForSpecialistResponse, "Ожидание ответа", "Ожидание ответа", "fa-inbox", null);
        await CheckAndInsertTicketStatus(ticketStatuses, TicketStatusConstants.MoreDetails, "Требуется уточнение", "Требуется уточнение", "fa fa-certificate", "label-danger");
        await CheckAndInsertTicketStatus(ticketStatuses, TicketStatusConstants.Answered, "Закрытые", "Закрыта", "fa-archive", "label-success");
        await CheckAndInsertTicketStatus(ticketStatuses, TicketStatusConstants.Draft, "Черновики", "Черновик", "fa-pen", "label-secondary");
    }
    private async Task CheckAndInsertTicketStatus(IEnumerable<TicketStatus> ticketStatuses, string statusName, string displayNameMany, string displayName, string icon, string style)
    {
        var status = ticketStatuses.Where(x => x.Name == statusName).FirstOrDefault();
        if (status == null)
        {
            await _ticketStatusRepository.InsertAsync(new TicketStatus
            {
                DisplayNameMany = displayNameMany,
                DisplayName = displayName,
                Icon = icon,
                IsPublic = true,
                Name = statusName,
                Style = style
            });
        }
    }
}
