namespace KV.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Dtos.Contacts;
using KV.Server.Dtos.Excel;
using KV.Server.Dtos.Tickets;
using KV.Server.Interfaces;
using KV.Server.Permissions;
using KV.Server.Permissions.Contracts;
using KV.Server.Permissions.Tickets;
using KV.Server.Profiles;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using OfficeOpenXml;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

public class ExcelTableAppService : ApplicationService, ITableAppService
{
    private readonly ITicketsAppService _ticketsAppService;
    private readonly ICrudContractService _crudContractAppService;
    private readonly ICrudCustomerAppService _crudCustomerAppService;
    private readonly IUsersAppService _usersAppService;
    private readonly IRepository<CustomerUserProfile, Guid> _customerUserProfilesRepository;
    private readonly IDataFilter _dataFilter;

    public ExcelTableAppService(ITicketsAppService ticketsAppService,
        ICrudContractService crudContractAppService,
        ICrudCustomerAppService crudCustomerAppService,
        IUsersAppService usersAppService,
        IRepository<CustomerUserProfile, Guid> customerUserProfilesRepository,
        IDataFilter dataFilter)
    {
        this._ticketsAppService = ticketsAppService;
        this._crudContractAppService = crudContractAppService;
        this._crudCustomerAppService = crudCustomerAppService;
        this._usersAppService = usersAppService;
        _customerUserProfilesRepository = customerUserProfilesRepository;
        _dataFilter = dataFilter;
    }

    [Authorize(TicketPermissions.Tickets.Get)]
    public async Task<IRemoteStreamContent> GenerateTicketTableAsync(GetTicketsListRequestDto input)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var fileName = $"Отчёт по заявкам от {DateTime.Now:dd.MM.yyyy}.xlsx";
        var fullName = Path.GetTempPath() + fileName;
        var file = new FileInfo(fullName);

        var pagedList = await this._ticketsAppService.GetTicketsListAsync(input);
        var tickets = pagedList.Items;
        using (var package = new ExcelPackage(file))
        {
            var worksheet = package.Workbook.Worksheets.Add($"Заявки {DateTime.Now:dd.MM.yyyy HH_mm ss}");
            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 80;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 15;
            worksheet.Column(6).Width = 25;
            worksheet.Column(7).Width = 20;
            worksheet.Cells[1, 1].Value = "Номер заявки";
            worksheet.Cells[1, 2].Value = "Наименование клиента";
            worksheet.Cells[1, 3].Value = "Тэг";
            worksheet.Cells[1, 4].Value = "Тип";
            worksheet.Cells[1, 5].Value = "Раздел";
            worksheet.Cells[1, 6].Value = "Специалист";
            worksheet.Cells[1, 7].Value = "Дата создания";
            for (var i = 0; i < tickets.Count; i++)
            {
                var ticket = tickets[i];
                worksheet.Cells[i + 2, 1].Value = ticket.Id;
                worksheet.Cells[i + 2, 2].Value = ticket.CustomerShortName;
                worksheet.Cells[i + 2, 3].Value = ticket.LastTagName;
                worksheet.Cells[i + 2, 4].Value = ticket.TicketTypeName;
                worksheet.Cells[i + 2, 5].Value = ticket.TicketSectionName;
                worksheet.Cells[i + 2, 6].Value = ticket.ResponsibleLastName;
                worksheet.Cells[i + 2, 7].Value = ticket.CreationTime.ToString("dd.MM.yyyy HH:mm");
            }
            await package.SaveAsync();
        }
        var stream = System.IO.File.Open(fullName, FileMode.OpenOrCreate);
        return new RemoteStreamContent(stream, fileName);
    }

    [Authorize(ContractPermissions.Contracts.Get)]
    public async Task<IRemoteStreamContent> GenerateContractTableAsync(GetContractListRequestDto input)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var fileName = $"Отчёт по контрактам от {DateTime.Now:dd.MM.yyyy}.xlsx";
        var fullName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
        var file = new FileInfo(fullName);

        input.MaxResultCount = 1;
        var pagedList = await this._crudContractAppService.GetListAsync(input);

        var contracts = new List<ContractDto>();

        var itemsLeft = pagedList.TotalCount;
        for (var chunkSize = 1000; itemsLeft >= 0; itemsLeft -= chunkSize)
        {
            input.MaxResultCount = chunkSize;
            input.SkipCount = Convert.ToInt32(pagedList.TotalCount - itemsLeft);
            var chunk = await _crudContractAppService.GetListAsync(input);

            if(chunk.Items.Any())
            {
                contracts.AddRange(chunk.Items);
            }
        }

        using (var package = new ExcelPackage(file))
        {
            var worksheet = package.Workbook.Worksheets.Add($"Контракты {DateTime.Now:dd.MM.yyyy HH_mm ss}");
            worksheet.Column(1).Width = 80;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 20;
            worksheet.Column(5).Width = 20;

            worksheet.Cells[1, 1].Value = "Название клиента";
            worksheet.Cells[1, 2].Value = "Номер договора";
            worksheet.Cells[1, 3].Value = "Пакет услуг";
            worksheet.Cells[1, 4].Value = "Дата начала";
            worksheet.Cells[1, 5].Value = "Дата окончания";
            for (var i = 0; i < contracts.Count; i++)
            {
                var contract = contracts[i];
                worksheet.Cells[i + 2, 1].Value = contract.TenantProfileShortName;
                worksheet.Cells[i + 2, 2].Value = contract.Name;
                worksheet.Cells[i + 2, 3].Value = JsonConvert.DeserializeObject<List<ServicePackageDataDto>>(contract.ServicePackagesData ?? string.Empty)?.Select(x=> x.Name)?.JoinAsString(" ,") ?? "Отсутствует";
                worksheet.Cells[i + 2, 4].Value = contract.ContractStartDate.ToString("dd.MM.yyyy");
                worksheet.Cells[i + 2, 5].Value = contract.ContractFinishDate.ToString("dd.MM.yyyy");
            }
            await package.SaveAsync();
        }
        var stream = System.IO.File.Open(fullName, FileMode.Open);
        return new RemoteStreamContent(stream, fileName);
    }

    [Authorize(CustomerPermissions.Profiles.Get)]
    public async Task<IRemoteStreamContent> GenerateCustomerTableAsync(GetCustomerListRequestDto input)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var fileName = $"Отчёт по клиентам от {DateTime.Now:dd.MM.yyyy}.xlsx";
        var fullName = Path.GetTempPath() + fileName;
        var file = new FileInfo(fullName);

        var pagedList = await this._crudCustomerAppService.GetListAsync(input);
        var customers = pagedList.Items;
        using (var package = new ExcelPackage(file))
        {
            var worksheet = package.Workbook.Worksheets.Add($"Клиенты {DateTime.Now:dd.MM.yyyy HH_mm ss}");
            worksheet.Column(1).Width = 70;
            worksheet.Column(2).Width = 70;
            worksheet.Column(3).Width = 60;
            worksheet.Cells[1, 1].Value = "Короткое Название";
            worksheet.Cells[1, 2].Value = "Полное Название";
            worksheet.Cells[1, 3].Value = "Адрес";
            for (var i = 0; i < customers.Count; i++)
            {
                var customer = customers[i];
                worksheet.Cells[i + 2, 1].Value = customer.ShortName;
                worksheet.Cells[i + 2, 2].Value = customer.LongName;
                worksheet.Cells[i + 2, 3].Value = customer.Address;
            }
            await package.SaveAsync();
        }
        var stream = System.IO.File.Open(fullName, FileMode.Open);
        return new RemoteStreamContent(stream, fileName);
    }

    [Authorize(UserPermissions.Profiles.Get)]
    public async Task<IRemoteStreamContent> GenerateUserTableAsync(GetUserListRequestDto input)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var fileName = $"Отчёт по пользователям от {DateTime.Now:dd.MM.yyyy}.xlsx";
        var fullName = Path.GetTempPath() + fileName;
        var file = new FileInfo(fullName);

        var pagedList = await this._usersAppService.GetListAsync(input);
        var users = pagedList.Items;
        using (var package = new ExcelPackage(file))
        {
            var worksheet = package.Workbook.Worksheets.Add($"Пользователи {DateTime.Now:dd.MM.yyyy HH_mm ss}");
            worksheet.Column(1).Width = 25;
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 65;
            worksheet.Column(5).Width = 15;
            worksheet.Cells[1, 1].Value = "ФИО";
            worksheet.Cells[1, 2].Value = "Должность";
            worksheet.Cells[1, 3].Value = "Направление деятельности";
            worksheet.Cells[1, 4].Value = "Наименование клиента";
            worksheet.Cells[1, 5].Value = "Актуализация";

            for (var i = 0; i < users.Count; i++)
            {
                var user = users[i];
                worksheet.Cells[i + 2, 1].Value = user.FullName;
                worksheet.Cells[i + 2, 2].Value = user.JobPost;
                worksheet.Cells[i + 2, 3].Value = user.Direction;
                worksheet.Cells[i + 2, 4].Value = user.TenantShortName;
                worksheet.Cells[i + 2, 5].Value = user.ActualizationTime.ToString("dd.MM.yyyy");
            }
            await package.SaveAsync();
        }
        var stream = System.IO.File.Open(fullName, FileMode.Open);
        return new RemoteStreamContent(stream, fileName);
    }

    [Authorize(CustomerPermissions.Profiles.Get)]
    public async Task<IRemoteStreamContent> GenerateStatisticsTableAsync(GetCustomerStatisticsListRequestDto input)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var fileName = $"Отчёт по статистике от {DateTime.Now:dd.MM.yyyy}.xlsx";
        var fullName = Path.GetTempPath() + fileName;
        var file = new FileInfo(fullName);

        var statistics = await this._crudCustomerAppService.GetStatisticsAsync(input);
        var month = new[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
        using (var package = new ExcelPackage(file))
        {
            var worksheet = package.Workbook.Worksheets.Add($"Статистика {DateTime.Now:dd.MM.yyyy HH_mm ss}");
            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 25;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 25;
            worksheet.Column(5).Width = 25;
            worksheet.Column(6).Width = 25;
            worksheet.Column(7).Width = 25;
            worksheet.Cells[1, 1].Value = "Раздел Название";
            worksheet.Cells[1, 2].Value = "Раздел количество";
            worksheet.Cells[1, 3].Value = "Тип Название";
            worksheet.Cells[1, 4].Value = "Тип Количество";
            worksheet.Cells[1, 5].Value = "Ограничение Название";
            worksheet.Cells[1, 6].Value = "Ограничение Количество";
            worksheet.Cells[1, 7].Value = "Дата";
            var skipCount = 0;
            for (var i = 0; i < statistics.Count; i++)
            {
                var statistic = statistics[i];
                for (var j = 0; j < statistic.TicketSectionsByMonths.Count; j++)
                {
                    var ticketSection = statistic.TicketSectionsByMonths[j];
                    worksheet.Cells[skipCount + j + 2, 1].Value = ticketSection.Name;
                    worksheet.Cells[skipCount + j + 2, 2].Value = ticketSection.Count;
                }
                for (var j = 0; j < statistic.TicketTypesByMonths.Count; j++)
                {
                    var ticketType = statistic.TicketTypesByMonths[j];
                    worksheet.Cells[skipCount + j + 2, 3].Value = ticketType.Name;
                    worksheet.Cells[skipCount + j + 2, 4].Value = ticketType.Count;
                }
                for (var j = 0; j < statistic.ConstraintsByMonths.Count; j++)
                {
                    var constraint = statistic.ConstraintsByMonths[j];
                    worksheet.Cells[skipCount + j + 2, 5].Value = constraint.Name;
                    worksheet.Cells[skipCount + j + 2, 6].Value = constraint.Sum;
                }
                var a = new List<int>() {
                    statistic.TicketSectionsByMonths.Count,
                    statistic.TicketTypesByMonths.Count,
                    statistic.ConstraintsByMonths.Count
                };
                for (var j = 0; j < a.Max(); j++)
                {
                    worksheet.Cells[skipCount + j + 2, 7].Value = $"{month[statistic.Month - 1]} месяц {statistic.Year} год";
                }
                skipCount += a.Max();
            }
            await package.SaveAsync();
        }
        var stream = System.IO.File.Open(fullName, FileMode.Open);
        return new RemoteStreamContent(stream, fileName);
    }

    [Authorize(CustomerPermissions.Profiles.Get)]
    public async Task<IRemoteStreamContent> GenerateCustomerUserProfileTableAsync(GetCustomerEmployeeTableDto input)
    {
        var customer = await _crudCustomerAppService.GetAsync(input.CustomerId);
        var customerUserProfiles = new List<CustomerUserProfile>();

        using (this._dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await this._customerUserProfilesRepository.WithDetailsAsync(x => x.TenantProfile);
            queryable.OrderByDescending(x => x.CreationTime); ;
            queryable = queryable.Where(x => x.TenantProfile.Id == input.CustomerId);

            var totalCount = queryable.Count();

            var itemsLeft = totalCount;

            for (var chunkSize = 1000; itemsLeft >= 0; itemsLeft -= chunkSize)
            {
                var entities = queryable
                    .Skip(totalCount - itemsLeft)
                    .Take(chunkSize)
                    .ToList();

                if (entities.Any())
                {
                    customerUserProfiles.AddRange(entities);
                }
            }
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var fileName = $"Отчёт по пользователям от {DateTime.Now:dd.MM.yyyy}.xlsx";
        var fullName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
        var file = new FileInfo(fullName);

        using (var package = new ExcelPackage(file))
        {
            var worksheet = package.Workbook.Worksheets.Add($"Пользователи {DateTime.Now:dd.MM.yyyy HH_mm ss}");
            worksheet.Column(1).Width = 25;
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 65;
            worksheet.Cells[1, 1].Value = "ФИО";
            worksheet.Cells[1, 2].Value = "Должность";
            worksheet.Cells[1, 3].Value = "Наименование клиента";

            for (var i = 0; i < customerUserProfiles.Count; i++)
            {
                var customerUser = customerUserProfiles[i];
                worksheet.Cells[i + 2, 1].Value = $"{customerUser.LastName ?? string.Empty} {customerUser.FirstName ?? string.Empty} {customerUser.MiddleName ?? string.Empty}";
                worksheet.Cells[i + 2, 2].Value = customerUser.JobPost;
                worksheet.Cells[i + 2, 3].Value = customer.DisplayName;
            }
            await package.SaveAsync();
        }
        var stream = System.IO.File.Open(fullName, FileMode.Open);
        return new RemoteStreamContent(stream, fileName);
    }
}
