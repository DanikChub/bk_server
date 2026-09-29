namespace KV.Server;
using System.Threading.Tasks;
using KV.Server.Dtos.Excel;
using KV.Server.Dtos.Tickets;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

public interface ITableAppService : IApplicationService
{
    Task<IRemoteStreamContent> GenerateTicketTableAsync(GetTicketsListRequestDto input);
    Task<IRemoteStreamContent> GenerateContractTableAsync(GetContractListRequestDto input);
    Task<IRemoteStreamContent> GenerateCustomerTableAsync(GetCustomerListRequestDto input);
    Task<IRemoteStreamContent> GenerateUserTableAsync(GetUserListRequestDto input);
    Task<IRemoteStreamContent> GenerateStatisticsTableAsync(GetCustomerStatisticsListRequestDto input);
    Task<IRemoteStreamContent> GenerateCustomerUserProfileTableAsync(GetCustomerEmployeeTableDto input);
}
