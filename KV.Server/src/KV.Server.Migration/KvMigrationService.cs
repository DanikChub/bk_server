namespace KV.Server.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using KV.Server.Contracts;
using KV.Server.EntityFrameworkCore;
using KV.Server.File;
using KV.Server.Migration.Extensions;
using KV.Server.Migration.Helpers;
using KV.Server.Migration.Models;
using KV.Server.Profiles;
using KV.Server.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

public class KvMigrationService : ITransientDependency
{
    public ILogger<KvMigrationService> Logger { get; set; }
    private readonly hdContentContext oldDb;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ITenantRepository _tenantsRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ITenantManager _tenantManager;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<TenantProfile, Guid> _tenantProfileRepository;
    private readonly IRepository<TicketStatus, Guid> _ticketStatusRepository;
    private readonly IRepository<Ticket, long> _ticketRepository;
    private readonly IRepository<TicketType, Guid> _ticketTypeRepository;
    private readonly IRepository<TicketHistory, Guid> _ticketHistoryRepository;
    private readonly IRepository<CustomerUserProfile, Guid> _customerUserProfileRepository;
    private readonly IRepository<ServicePackage, Guid> _servicePackageRepository;
    private readonly IRepository<UploadedFile, Guid> _uploadedFileRepository;
    private readonly IRepository<Tenants.Region, Guid> _regionRepository;
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IRepository<TicketAttachment> _ticketAttachmentRepository;
    private readonly IRepository<TicketHistoryAttachment> _ticketHistoryAttachmentRepository;
    private readonly IRepository<ContractServicePackage> _contractServicePackageRepository;
    private readonly IRepository<ContractStatus, Guid> _contractStatusRepository;
    private readonly IRepository<IdentityUser, Guid> _identityUserRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IDbContextProvider<ServerDbContext> _dbContextProvider;
    public KvMigrationService(
        hdContentContext oldDatabase,
        IUnitOfWorkManager unitOfWorkManager,
        ITenantRepository tenantsRepository,
        IGuidGenerator guidGenerator,
        ITenantManager tenantManager,
        IdentityRoleManager identityRoleManager,
        IdentityUserManager identityUserManager,
        ICurrentTenant currentTenant,
        IRepository<TenantProfile, Guid> tenantProfileRepository,
        IRepository<TicketStatus, Guid> ticketStatusRepository,
        IRepository<Ticket, long> ticketRepository,
        IRepository<TicketType, Guid> ticketTypeRepository,
        IRepository<TicketHistory, Guid> ticketHistoryRepository,
        IRepository<CustomerUserProfile, Guid> customerUserProfileRepository,
        IRepository<ServicePackage, Guid> servicePackageRepository,
        IRepository<UploadedFile, Guid> uploadedFileRepository,
        IRepository<Tenants.Region, Guid> regionRepository,
        IRepository<Contract, Guid> contractRepository,
        IRepository<TicketAttachment> ticketAttachmentRepository,
        IRepository<TicketHistoryAttachment> ticketHistoryAttachmentRepository,
        IRepository<ContractServicePackage> contractServicePackageRepository,
        IRepository<ContractStatus, Guid> contractStatusRepository,
        IRepository<IdentityUser, Guid> identityUserRepository,
        IDataFilter dataFilter,
        IDbContextProvider<ServerDbContext> dbContextProvider
    )
    {
        this.oldDb = oldDatabase;
        this._unitOfWorkManager = unitOfWorkManager;
        this._tenantsRepository = tenantsRepository;
        this._guidGenerator = guidGenerator;
        this._tenantManager = tenantManager;
        this._identityRoleManager = identityRoleManager;
        this._identityUserManager = identityUserManager;
        this._currentTenant = currentTenant;
        this._tenantProfileRepository = tenantProfileRepository;
        this._ticketStatusRepository = ticketStatusRepository;
        this._ticketRepository = ticketRepository;
        this._ticketTypeRepository = ticketTypeRepository;
        this._ticketHistoryRepository = ticketHistoryRepository;
        this._customerUserProfileRepository = customerUserProfileRepository;
        this._servicePackageRepository = servicePackageRepository;
        this._uploadedFileRepository = uploadedFileRepository;
        this._regionRepository = regionRepository;
        this._contractRepository = contractRepository;
        this._ticketAttachmentRepository = ticketAttachmentRepository;
        this._ticketHistoryAttachmentRepository = ticketHistoryAttachmentRepository;
        this._contractServicePackageRepository = contractServicePackageRepository;
        this._contractStatusRepository = contractStatusRepository;
        this._identityUserRepository = identityUserRepository;
        this._dataFilter = dataFilter;
        this._dbContextProvider = dbContextProvider;
    }
    private readonly string TenantCustomerName = "CUSTOMER_";
    private readonly string TenantUserName = "USER_";
    private readonly Guid TicketTypeNewId = Guid.Parse("a736e1b2-3ad6-471f-971f-d2a27f86a417");

    private async Task MigrateRegionAsync()
    {
        List<string> existingRegions;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            existingRegions = (await this._regionRepository.GetListAsync())
                .Select(p => p.Name).ToList();
        }

        var regionsOld = await this.oldDb.Regions.ToListAsync();
        foreach (var regionOld in regionsOld)
        {
            try
            {
                var regionName = regionOld.Title;
                var existing = existingRegions.Any(p => p == regionName);
                // если регион уже существует в целевой бд
                if (!existing)
                {
                    var region = new Tenants.Region()
                    {
                        Name = regionOld.Title,
                        Code = regionOld.Code
                    };
                    using (var uow = this._unitOfWorkManager.Begin())
                    {
                        await this._regionRepository.InsertAsync(region);
                        await uow.SaveChangesAsync();
                    }
                }
            }
            catch (Exception exc)
            {
                this.Logger.LogException(exc);
            }
        }
    }

    private async Task MigrateTicketStatusAsync()
    {
        List<string> existingTicketStatuses;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            existingTicketStatuses = (await this._ticketStatusRepository.GetListAsync())
                .Select(p => p.Name).ToList();
        }

        var ticketStatusesOld = await this.oldDb.HdticketStatuses.ToListAsync();
        var TicketStatusFirstId = Guid.Parse("96236a52-d759-40ce-b063-01d8c85abcc4");
        foreach (var ticketStatusOld in ticketStatusesOld)
        {
            try
            {
                var statusName = ticketStatusOld.Name;
                var existing = existingTicketStatuses
                    .Any(p => p == statusName);
                // если статус уже существует в целевой бд
                if (!existing)
                {
                    var ticketStatus = new TicketStatus()
                    {
                        DisplayName = ticketStatusOld.DisplayName,
                        IsPublic = ticketStatusOld.IsPublic,
                        Name = ticketStatusOld.Name
                    };
                    if (ticketStatusOld.Id == 1)
                    {
                        ticketStatus.SetId(TicketStatusFirstId);
                    }
                    using (var uow = this._unitOfWorkManager.Begin())
                    {
                        await this._ticketStatusRepository.InsertAsync(ticketStatus);
                        await uow.SaveChangesAsync();
                    }
                }
            }
            catch (Exception exc)
            {
                this.Logger.LogException(exc);
            }
        }
    }

    private async Task MigrateServicePackageAsync()
    {
        List<string> existingServicePackages;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            existingServicePackages = (await this._servicePackageRepository.GetListAsync())
                .Select(p => p.Name).ToList();
        }

        var servicePackagesOld = await this.oldDb.HdservicePackages.ToListAsync();
        foreach (var servicePackageOld in servicePackagesOld)
        {
            try
            {
                var serviceName = servicePackageOld.Name;
                var existing = existingServicePackages
                    .Any(p => p == serviceName);
                // если услуга уже существует в целевой бд
                if (!existing)
                {
                    var ServicePackage = new ServicePackage()
                    {
                        Name = servicePackageOld.Name
                    };
                    using (var uow = this._unitOfWorkManager.Begin())
                    {
                        await this._servicePackageRepository.InsertAsync(ServicePackage);
                        await uow.SaveChangesAsync();
                    }
                }
            }
            catch (Exception exc)
            {
                this.Logger.LogException(exc);
            }
        }
    }

    private async Task MigrateTenantsAsync()
    {
        List<string> existingTenants;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            existingTenants = (await this._tenantsRepository.GetListAsync())
                .Select(p => p.Name).ToList();
        }
        var vendorsOld = await this.oldDb.Vendors.Include(x => x.Region).ToListAsync();
        var lastSaveDateCustomer = new DateTime(2016, 12, 9, 13, 45, 10);
        foreach (var vendorOld in vendorsOld)
        {
            try
            {
                var tenantName = $"{this.TenantCustomerName}{vendorOld.Id}";
                var existing = existingTenants.Any(p => p == tenantName);
                // если тенант уже существует в целевой бд
                if (!existing)
                {
                    using (var uow = this._unitOfWorkManager.Begin())
                    {
                        var userNameOrEmail = string.IsNullOrEmpty(vendorOld.DirectorLogin?.Trim()) ?
                            vendorOld.DirectorEmail : vendorOld.DirectorLogin;

                        IdentityUser userExists;
                        using (this._dataFilter.Disable<IMultiTenant>())
                        {
                            userExists = await this._identityUserRepository.FirstOrDefaultAsync(x => x.UserName == userNameOrEmail);
                        }
                        if (userExists == null)
                        {
                            Guid tenantId;
                            using (this._currentTenant.Change(null))
                            {
                                var tenant = await this._tenantManager.CreateAsync(tenantName);
                                await this._tenantsRepository.InsertAsync(tenant);
                                await uow.SaveChangesAsync();
                                tenantId = tenant.Id;
                            }

                            using (this._currentTenant.Change(tenantId))
                            {
                                var user = new CustomerUserProfile(Guid.NewGuid(), Guid.NewGuid(), null, null);
                                user.SetFullName(vendorOld.ContractOwner, null, null)
                                    .SetEmail(vendorOld.ContactEmail);

                                var tenantProfile = new TenantProfile(tenantId,
                                    vendorOld.ShortName, vendorOld.OfficialName, vendorOld.Address)
                                {
                                    Description = vendorOld.Description,
                                    INNNumber = vendorOld.Innnumber,
                                    KPPNumber = vendorOld.Kppnumber,
                                    DisplayName = vendorOld.DisplayName,
                                    SiteUrl = vendorOld.SiteUrl == "нет" ? null : vendorOld.SiteUrl,
                                    IsActive = vendorOld.IsActive,
                                    CommentNotes = vendorOld.CommentNotes,
                                    ContactUserProfile = user,
                                };
                                var regionName = vendorOld.Region?.Title;
                                tenantProfile.RegionId = (await this._regionRepository.GetAsync(x => x.Name == regionName)).Id;

                                // create tenant profile
                                var createdTenantProfile = await this._tenantProfileRepository.InsertAsync(tenantProfile);
                                await uow.SaveChangesAsync();

                                // create admin user
                                var identity = new IdentityUser(this._guidGenerator.Create(), userNameOrEmail, vendorOld.DirectorEmail, tenantId);

                                var userNameIdentity = identity.UserName ?? identity.Email;
                                var customerOldExists = await this.oldDb.Hdprofiles.Include(x => x.User)
                                    .FirstOrDefaultAsync(x => x.User.UserName == userNameIdentity);

                                string customerNote = null;
                                if (vendorOld.DirectorPhone?.Length > 16)
                                {
                                    customerNote = vendorOld.DirectorPhone;
                                }
                                else
                                {
                                    identity.SetPhoneNumber(vendorOld.DirectorPhone, false);
                                    identity.SetPhoneNumberConfirmed(customerOldExists?.User?.PhoneNumberConfirmed ?? false);
                                }
                                identity.SetIsActive(vendorOld.IsActive);
                                try
                                {
                                    var (LastName, FirstName, MiddleName) = UtilsMigration.GetFioByFullName(vendorOld.DirectorFullName);
                                    identity.Name = FirstName;
                                    identity.Surname = LastName;
                                }
                                catch { }
                                identity.Name = UtilsMigration.GetNullIfNullOrWhiteSpace(identity.Name);
                                identity.Surname = UtilsMigration.GetNullIfNullOrWhiteSpace(identity.Surname);

                                var passwordHash = customerOldExists?.User?.PasswordHash;
                                if (passwordHash == null) // 100 пользователей всё равно без пароля!
                                {
                                    var identityOld = await this.oldDb.AspNetUsers.FirstOrDefaultAsync(x =>
                                        x.UserName == userNameOrEmail &&
                                        x.FirstName == identity.Name &&
                                        x.Email == identity.Email);
                                    passwordHash = identityOld?.PasswordHash;
                                }

                                identity.SetPasswordHash(passwordHash);
                                if (customerOldExists == null)
                                {
                                    lastSaveDateCustomer.AddDays(1.25);
                                    identity.CreationTime = lastSaveDateCustomer;
                                    identity.LastModificationTime = null;
                                }
                                else
                                {
                                    identity.CreationTime = customerOldExists.Created;
                                    identity.LastModificationTime = customerOldExists.Updated;
                                }

                                var identityResult = await this._identityUserManager.CreateAsync(identity);
                                if (!identityResult.Succeeded)
                                {
                                    this.Logger.LogError("Failed to created Identity user", identityResult);
                                }
                                else
                                {
                                    await this._identityRoleManager.CreateAsync(
                                        new IdentityRole(this._guidGenerator.Create(), "admin", tenantId));
                                    await uow.SaveChangesAsync();

                                    var userName = string.IsNullOrEmpty(vendorOld.DirectorLogin?.Trim()) ?
                                        vendorOld.DirectorEmail : vendorOld.DirectorLogin;
                                    var identityUser = await this._identityUserManager.FindByNameAsync(userName);
                                    var roleAssignResult = await this._identityUserManager.AddToRoleAsync(identityUser, "admin");
                                    if (!roleAssignResult.Succeeded)
                                    {
                                        this.Logger.LogError("Failed to assign role to user", roleAssignResult);
                                    }

                                    var customer = new CustomerUserProfile(this._guidGenerator.Create(), identityUser.Id, null, identityUser.TenantId);
                                    customer.WriteNote(customerNote);
                                    customer.SetAddress("Россия", null, null);
                                    if (customerOldExists == null)
                                    {
                                        customer.SetJobPost("Директор");
                                        customer.SetCreationTime(identity.CreationTime);
                                    }
                                    else
                                    {
                                        lastSaveDateCustomer = customerOldExists.Created;
                                        customer.SetJobPost(customerOldExists.JobPost);
                                        customer.SetCreationTime(customerOldExists.Created);
                                        customer.SetLastModificationTime(customerOldExists.Updated);
                                        customer.SetDateOfBirthday(customerOldExists.DateOfBirth);
                                    }
                                    try
                                    {
                                        var (LastName, FirstName, MiddleName) = UtilsMigration.GetFioByFullName(vendorOld.DirectorFullName);
                                        customer.SetFullName(LastName, FirstName, MiddleName);
                                    }
                                    catch { }
                                    await this._customerUserProfileRepository.InsertAsync(customer);
                                    await uow.SaveChangesAsync();
                                }
                                await uow.SaveChangesAsync();
                            }
                        }
                        await uow.CompleteAsync();
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Не мигрировал тенанта");
                this.Logger.LogException(exc);
            }
        }
    }

    private async Task MigrateCustomerUserProfileAsync()
    {
        List<CustomerUserProfile> existingCustomers;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            using (this._dataFilter.Disable<IMultiTenant>())
            {
                existingCustomers = await (await this._customerUserProfileRepository
                    .WithDetailsAsync(x => x.IdentityUser))
                    .ToListAsync();
            }
        }

        var customersOld = await this.oldDb.Hdprofiles
            .Include(x => x.User)
            .Include(x => x.Vendor)
            .ToListAsync();

        foreach (var customerOld in customersOld)
        {
            var tenantName = $"{this.TenantUserName}{customerOld.Id}";
            var identityUserOld = await this.oldDb.AspNetUsers
                .Include(x => x.Hdprofiles)
                .FirstOrDefaultAsync(c => c.Id == customerOld.UserId);

            var existing = existingCustomers
                .Any(t => t.IdentityUser.UserName == customerOld.User.UserName);
            if (!existing)
            {
                using (var uow = this._unitOfWorkManager.Begin())
                {
                    var tenant = await this._tenantProfileRepository.FirstOrDefaultAsync(x => x.Address == customerOld.Vendor.Address
                        && customerOld.Vendor.Innnumber == x.INNNumber);
                    Guid? tenantId = tenant.Id;
                    // special user
                    if (customerOld.Vendor.Id == 87)
                    {
                        tenantId = null;
                        continue;
                    }

                    var identityUser = new IdentityUser(this._guidGenerator.Create(),
                    string.IsNullOrEmpty(customerOld.User.UserName?.Trim()) ?
                        customerOld.User.Email : customerOld.User.UserName,
                        $"{customerOld.User.UserName?.Trim()}@kv.system", tenantId);

                    string customerNote = null;
                    if (identityUserOld.PhoneNumber?.Length > 16)
                    {
                        customerNote = identityUserOld.PhoneNumber;
                    }
                    else
                    {
                        identityUser.SetPhoneNumber(identityUserOld.PhoneNumber, identityUserOld.PhoneNumberConfirmed);
                    }
                    identityUser.SetEmailConfirmed(identityUserOld.EmailConfirmed);
                    identityUser.SetPhoneNumberConfirmed(identityUserOld.PhoneNumberConfirmed);
                    identityUser.CreationTime = customerOld.Created;
                    identityUser.LastModificationTime = customerOld.Updated;
                    identityUser.SetPasswordHash(customerOld?.User?.PasswordHash);
                    var fullName = $"{identityUserOld.LastName} {identityUserOld.FirstName} {identityUserOld.MiddleName}";
                    try
                    {
                        var (LastName, FirstName, MiddleName) = UtilsMigration.GetFioByFullName(fullName);
                        identityUserOld.FirstName = UtilsMigration.GetNullIfNullOrWhiteSpace(FirstName);
                        identityUserOld.LastName = UtilsMigration.GetNullIfNullOrWhiteSpace(LastName);
                        identityUserOld.MiddleName = UtilsMigration.GetNullIfNullOrWhiteSpace(MiddleName);
                    }
                    catch { }
                    identityUserOld.FirstName = UtilsMigration.GetNullIfNullOrWhiteSpace(identityUserOld.FirstName);
                    identityUserOld.LastName = UtilsMigration.GetNullIfNullOrWhiteSpace(identityUserOld.LastName);
                    identityUserOld.MiddleName = UtilsMigration.GetNullIfNullOrWhiteSpace(identityUserOld.MiddleName);
                    identityUser.Name = identityUserOld.FirstName;
                    identityUser.Surname = identityUserOld.LastName;
                    await this._identityUserRepository.InsertAsync(identityUser);
                    await uow.SaveChangesAsync();

                    var customer = new CustomerUserProfile(this._guidGenerator.Create(), 
                        identityUser.Id, null, tenantId);
                    customer.WriteNote(customerNote);
                    customer.SetDateOfBirthday(customerOld.DateOfBirth);
                    customer.SetAddress("Россия", null, null);
                    customer.SetFullName(identityUserOld.LastName, identityUserOld.FirstName, identityUserOld.MiddleName);
                    customer.SetJobPost(customerOld.JobPost);
                    customer.SetCreationTime(customerOld.Created);
                    customer.SetLastModificationTime(customerOld.Updated);
                    customer.SetEmail(customerOld.User.Email);
                    await this._customerUserProfileRepository.InsertAsync(customer);
                    await uow.SaveChangesAsync();
                }
            }
        }
    }

    //TODO: Id (не сортируется)
    private async Task MigrateTicketsAsync(int take = int.MaxValue / 2, int skip = 0)
    {
        List<Ticket> existingTickets;
        List<IdentityUser> identityUsers;
        List<CustomerUserProfile> customerProfiles;
        List<TicketStatus> ticketStatuses;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            existingTickets = await this._ticketRepository.ToListAsync();
            ticketStatuses = await this._ticketStatusRepository.ToListAsync();
            using (this._dataFilter.Disable<IMultiTenant>())
            {
                identityUsers = await this._identityUserRepository.ToListAsync();
                customerProfiles = await (await this._customerUserProfileRepository
                    .WithDetailsAsync(x => x.IdentityUser))
                    .ToListAsync();
            }
        }
        if (take > 30000)
        {
            Console.WriteLine("Wait long time many ticket with many reference...");
        }

        var ticketStatusesOld = await this.oldDb.HdticketStatuses.ToListAsync();
        var ticketsOld = await this.oldDb.Hdtickets
            .Include(x => x.AssignedTo)
            .Include(x => x.Author)
            .ThenInclude(x => x.User)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        foreach (var ticketOld in ticketsOld)
        {
            var existing = existingTickets
                .Any(t => t.Id == ticketOld.Id);
            if (!existing)
            {
                using (var uow = this._unitOfWorkManager.Begin())
                {
                    var userNameAuthor = string.IsNullOrWhiteSpace(ticketOld.Author.User.UserName)
                        ? ticketOld.Author.User.Email : ticketOld.Author.User.UserName;
                    var userNameResponsible = string.IsNullOrWhiteSpace(ticketOld?.AssignedTo?.UserName)
                        ? ticketOld.AssignedTo?.Email : ticketOld.AssignedTo?.UserName;
                    var author = identityUsers.FirstOrDefault(x => x.UserName == userNameAuthor);
                    var responsible = customerProfiles.FirstOrDefault(x => x.IdentityUser.UserName == userNameResponsible);
                    using (this._currentTenant.Change(author?.TenantId))
                    {
                        var ticketStatusOld = ticketStatusesOld.FirstOrDefault(x => x.Id == ticketOld.StatusId);
                        var ticketStatusId = ticketStatuses.FirstOrDefault(x => x.Name == ticketStatusOld.Name).Id;
                        var ticketTypeId = this.TicketTypeNewId;
                        var ticket = new Ticket(ticketOld.Id, ticketOld.Subject ?? "", ticketOld.Body ?? "", ticketTypeId, ticketStatusId, responsible?.Id);
                        ticket.SetCreationTime(ticketOld.Created);
                        ticket.SetLastModificationTime(ticketOld.Updated);
                        ticket.SetRating(ticketOld.Rating);
                        ticket.SetDueDate(ticketOld.DueDate);
                        ticket.CreatorId = author?.Id;
                        await this._ticketRepository.InsertAsync(ticket);
                        await uow.SaveChangesAsync();
                    }
                }
            }
        }
    }

    private async Task MigrateTicketHistoryAsync(int take = int.MaxValue / 2, int skip = 0)
    {
        List<TicketHistory> existingTicketsHistory;
        List<TicketStatus> ticketStatuses;
        List<IdentityUser> identityUsers;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            existingTicketsHistory = await this._ticketHistoryRepository.ToListAsync();
            ticketStatuses = await this._ticketStatusRepository.ToListAsync();
            using (this._dataFilter.Disable<IMultiTenant>())
            {
                //TODO: this fast, existingTicketsHistory = await (await _ticketHistoryRepository.GetQueryableAsync()).Skip(skip).Take(take).ToListAsync();
                identityUsers = await this._identityUserRepository.ToListAsync();
            }
        }

        var ticketStatusesOld = await this.oldDb.HdticketStatuses.ToListAsync();
        var ticketsHistoryOld = await this.oldDb.HdticketStatusHistories
            .Include(x => x.Author)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        foreach (var ticketHistoryOld in ticketsHistoryOld)
        {
            var existing = existingTicketsHistory
                .Any(t => t.CreationTime == ticketHistoryOld.Created &&
                t.Description == ticketHistoryOld.Comment);

            if (!existing)
            {
                using (var uow = this._unitOfWorkManager.Begin())
                {
                    var userNameAuthor = string.IsNullOrWhiteSpace(ticketHistoryOld.Author.UserName)
                        ? ticketHistoryOld.Author.Email : ticketHistoryOld.Author.UserName;
                    var author = identityUsers.FirstOrDefault(x => x.UserName == userNameAuthor);
                    using (this._currentTenant.Change(author?.TenantId))
                    {
                        var ticketStatusOld = ticketStatusesOld.FirstOrDefault(x => x.Id == ticketHistoryOld.StatusId);
                        var ticketStatus = ticketStatuses.FirstOrDefault(x => x.Name == ticketStatusOld.Name);
                        var ticketHistoryType = TicketHistoryType.SpecialistMessage;
                        if (ticketHistoryOld.IsCustomerMessage)
                        {
                            ticketHistoryType = TicketHistoryType.CustomerMessage;
                        }
                        var ticketHistory = new TicketHistory(author?.Id, ticketHistoryOld.Comment, (long)ticketHistoryOld.TicketId, ticketStatus.Id, ticketHistoryType);
                        ticketHistory.SetCreationTime(ticketHistoryOld.Created);
                        await this._ticketHistoryRepository.InsertAsync(ticketHistory);
                        await uow.SaveChangesAsync();
                    }
                }
            }
        }
    }

    //TODO: CreatorId - нельзя вычислить так как там ссылка на компанию тока новые записи можно вычислить
    private async Task MigrateContractAndContractServiceAsync()
    {
        var contractsOld = await this.oldDb.HdservicePackageVendors
            .Include(x => x.ServicePackage)
            .Include(x => x.Vendor).ToListAsync();
        foreach (var contractOld in contractsOld)
        {
            using (var uow = this._unitOfWorkManager.Begin())
            {
                var contractExisting = await this._contractRepository.FirstOrDefaultAsync(x =>
                    x.ContractStartDate == contractOld.ContractStart &&
                    x.ContractFinishDate == contractOld.ContractEnd);
                // если контракт не существует в целевой бд
                if (contractExisting == null)
                {
                    var tenantProfile = await this._tenantProfileRepository.FirstOrDefaultAsync(x =>
                        x.DisplayName == contractOld.Vendor.DisplayName &&
                        x.Address == contractOld.Vendor.Address);
                    var contract = new Contract()
                    {
                        Name = contractOld.ContractNumber ?? "Contract",
                        ContractStartDate = contractOld.ContractStart,
                        ContractFinishDate = contractOld.ContractEnd,
                        TenantId = tenantProfile.Id,
                        IsArchive = !contractOld.IsActive,
                        IsDraft = contractOld.NotSigned
                    };
                    contract.SetCreationTime(contractOld.ContractCreated ?? contractOld.ContractStart);

                    ContractStatus contractStatus;
                    using (this._dataFilter.Disable<IMultiTenant>())
                    {
                        if (contractOld.ContractCreated == null)
                        {
                            contractStatus = await this._contractStatusRepository.FirstOrDefaultAsync(x => x.Code == "draft");
                        }
                        else if (DateTime.Now > contractOld.ContractEnd)
                        {
                            contractStatus = await this._contractStatusRepository.FirstOrDefaultAsync(x => x.Code == "complete");
                        }
                        else
                        {
                            contractStatus = await this._contractStatusRepository.FirstOrDefaultAsync(x => x.Code == "in progress");
                        }
                    }
                    contract.StatusId = contractStatus.Id;
                    await this._contractRepository.InsertAsync(contract);

                    var servicePackage = await this._servicePackageRepository.FirstOrDefaultAsync(x => x.Name == contractOld.ServicePackage.Name);
                    var contractService = new ContractServicePackage
                    {
                        ContractId = contract.Id,
                        ServicePackageId = servicePackage.Id,
                        ContractStartDate = contract.ContractStartDate,
                        ContractFinishDate = contract.ContractFinishDate
                    };
                    await this._contractServicePackageRepository.InsertAsync(contractService);
                    await uow.SaveChangesAsync();
                }
            }
        }
    }

    private async Task MigrateFileAsync()
    {
        List<UploadedFile> existingUploadedFile;
        List<IdentityUser> identityUsers;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            existingUploadedFile = await this._uploadedFileRepository.ToListAsync();
            using (this._dataFilter.Disable<IMultiTenant>())
            {
                identityUsers = await this._identityUserRepository.ToListAsync();
            }
        }

        var mediaFilesOld = await this.oldDb.MediaFiles
            .Include(x => x.Author)
            .ToListAsync();
        foreach (var mediaFileOld in mediaFilesOld)
        {
            using (var uow = this._unitOfWorkManager.Begin())
            {
                var existing = existingUploadedFile
                    .Any(t => t.CreationTime == mediaFileOld.Created &&
                    t.FileName == mediaFileOld.FileName);

                if (!existing)
                {
                    var userNameAuthor = string.IsNullOrWhiteSpace(mediaFileOld.Author.UserName)
                        ? mediaFileOld.Author.Email : mediaFileOld.Author.UserName;
                    var author = identityUsers.FirstOrDefault(x => x.UserName == userNameAuthor);
                    using (this._currentTenant.Change(author?.TenantId))
                    {
                        var uploadedFile = new UploadedFile()
                        {
                            ContentType = mediaFileOld.MediaType,
                            SizeInBytes = mediaFileOld.Size,
                            FileName = mediaFileOld.FileName,
                            Title = mediaFileOld.DisplayName,
                            OriginalFileName = mediaFileOld.OriginalFileName ??
                                UtilsMigration.MediaFileSplitDisplayName(mediaFileOld.DisplayName, mediaFileOld.Created)
                        };

                        uploadedFile.SetCreationTime(mediaFileOld.Created);
                        uploadedFile.SetCreatorId(author?.Id);

                        await this._uploadedFileRepository.InsertAsync(uploadedFile);
                        await uow.SaveChangesAsync();
                    }
                }
            }
        }
    }

    private async Task MigrateTicketAttachmentAsync(int take = int.MaxValue / 2, int skip = 0)
    {
        List<UploadedFile> uploadedFiles;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            uploadedFiles = await this._uploadedFileRepository.ToListAsync();
        }
        var ticketAttachmentsOld = await this.oldDb.HdticketAttachments
            .Include(x => x.MediaFileInfo)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        foreach (var ticketAttachmentOld in ticketAttachmentsOld)
        {
            var uploadedFile = uploadedFiles.FirstOrDefault(x =>
                x.CreationTime == ticketAttachmentOld.MediaFileInfo.Created &&
                x.FileName == ticketAttachmentOld.MediaFileInfo.FileName);
            if (uploadedFile != null)
            {
                using (var uow = this._unitOfWorkManager.Begin())
                {
                    TicketAttachment existsAttachment;
                    using (this._dataFilter.Disable<IMultiTenant>())
                    {
                        existsAttachment = await this._ticketAttachmentRepository.FirstOrDefaultAsync(x =>
                            x.TicketId == ticketAttachmentOld.TicketId && x.UploadedFileId == uploadedFile.Id);
                    }
                    // если вложеность не существует в целевой бд
                    if (existsAttachment == null)
                    {
                        Ticket ticketSelect;
                        using (this._dataFilter.Disable<IMultiTenant>())
                        {
                            ticketSelect = await this._ticketRepository.FirstOrDefaultAsync(x => x.Id == ticketAttachmentOld.TicketId);
                        }

                        var ticketAttachment = new TicketAttachment()
                        {
                            TicketId = ticketAttachmentOld.TicketId,
                            UploadedFileId = uploadedFile.Id
                        };
                        ticketSelect.SetAttachmentsCount(ticketSelect.AttachmentsCount + 1);
                        await this._ticketRepository.UpdateAsync(ticketSelect);
                        await this._ticketAttachmentRepository.InsertAsync(ticketAttachment);
                        await uow.SaveChangesAsync();
                    }
                }
            }
            else
            {
                Console.WriteLine($"Не мигрировали вложение TicketAttachment");
            }
        }
    }

    private async Task MigrateTicketHistoryAttachmentAsync(int take = int.MaxValue / 2, int skip = 0)
    {
        List<UploadedFile> uploadedFiles;
        List<TicketHistory> ticketHistories;
        using (var uow = this._unitOfWorkManager.Begin())
        {
            using (this._dataFilter.Disable<IMultiTenant>())
            {
                uploadedFiles = await this._uploadedFileRepository.ToListAsync();
                ticketHistories = await this._ticketHistoryRepository.ToListAsync();
            }
        }

        var uploadedFilesOld = await this.oldDb.MediaFiles
            .Include(x => x.HdticketStatusHistoryItem)
            .Where(x => x.HdticketStatusHistoryItemId != null)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        foreach (var uploadedFileOld in uploadedFilesOld)
        {
            var uploadedFile = uploadedFiles.FirstOrDefault(x =>
                x.SizeInBytes == uploadedFileOld.Size &&
                x.CreationTime.ToString("MM.dd.yyyy HH:mm") == uploadedFileOld.Created.ToString("MM.dd.yyyy HH:mm"));
            var ticketHistory = ticketHistories.FirstOrDefault(x =>
                x.TicketId == uploadedFileOld.HdticketStatusHistoryItem.TicketId &&
                x.Description == uploadedFileOld.HdticketStatusHistoryItem.Comment);

            if (ticketHistory != null && uploadedFile != null)
            {
                using (var uow = this._unitOfWorkManager.Begin())
                {
                    var existsAttachment = await this._ticketHistoryAttachmentRepository.FirstOrDefaultAsync(x =>
                        x.TicketHistoryId == ticketHistory.Id && x.UploadedFileId == uploadedFile.Id);
                    // если вложеность не существует в целевой бд
                    if (existsAttachment == null)
                    {
                        var ticketHistoryAttachment = new TicketHistoryAttachment()
                        {
                            TicketHistoryId = ticketHistory.Id,
                            UploadedFileId = uploadedFile.Id
                        };
                        await this._ticketHistoryAttachmentRepository.InsertAsync(ticketHistoryAttachment);
                        await uow.SaveChangesAsync();
                    }
                }
            }
            else
            {
                Console.WriteLine($"Не мигрировали вложение TicketAttachment {(ticketHistory == null ? "ticketHistory" : "")} {(uploadedFile == null ? "uploadedFile" : "")}");
            }
        }
    }

    private async Task MigrateTicketTypeAsync()
    {
        using (var uow = this._unitOfWorkManager.Begin())
        {
            try
            {
                var ticketTypeName = "New";
                var ticketTypeId = this.TicketTypeNewId;
                var ticketTypeExists = await this._ticketTypeRepository.FirstOrDefaultAsync(x => x.Id == ticketTypeId);
                if (ticketTypeExists == null)
                {
                    var ticketType = new TicketType()
                    {
                        Name = ticketTypeName
                    };
                    ticketType.SetId(ticketTypeId);

                    await this._ticketTypeRepository.InsertAsync(ticketType);
                }
            }
            catch { }
            await uow.SaveChangesAsync();
        }
    }

    private async Task MigrateContractStatusAsync()
    {
        using (var uow = this._unitOfWorkManager.Begin())
        {
            try
            {
                var contractStatuses = new List<ContractStatus>()
                {
                    new ContractStatus() { Title = "Отменён", Code = "cancel", TenantId = null },
                    new ContractStatus() { Title = "Выполнен", Code = "complete", TenantId = null },
                    new ContractStatus() { Title = "Черновик", Code = "draft", TenantId = null }
                };
                foreach (var contractStatus in contractStatuses)
                {
                    contractStatus.SetId(Guid.NewGuid());
                }

                var contractStatusProgress = new ContractStatus()
                {
                    Title = "В ходе выполнения",
                    Code = "in progress",
                    TenantId = null
                };
                contractStatusProgress.SetId(Guid.Parse("3ad2fde8-8542-9432-b650-cea5967ee1bd"));
                contractStatuses.Insert(0, contractStatusProgress);

                foreach (var contractStatus in contractStatuses)
                {
                    ContractStatus contractStatusExists;
                    using (this._dataFilter.Disable<IMultiTenant>())
                    {
                        contractStatusExists = await this._contractStatusRepository.FirstOrDefaultAsync(x => x.Code == contractStatus.Code);
                    }
                    if (contractStatusExists == null)
                    {
                        var contractStatusNew = new ContractStatus()
                        {
                            Title = contractStatus.Title,
                            TenantId = contractStatus.TenantId,
                            Code = contractStatus.Code
                        };
                        contractStatusNew.SetId(contractStatus.Id);
                        await this._contractStatusRepository.InsertAsync(contractStatusNew);
                    }
                }
            }
            catch { }
            await uow.SaveChangesAsync();
        }
    }

    private async Task MigrateAutoIncrementFixAsync()
    {
        using (var uow = this._unitOfWorkManager.Begin())
        {
            var ticketId = this.oldDb.Hdtickets.Max(x => x.Id) + 1;
            (await this._dbContextProvider.GetDbContextAsync()).Database.ExecuteSqlRaw("ALTER TABLE IF EXISTS public.\"Tickets\" ALTER COLUMN \"Id\" RESTART SET START " + ticketId.ToString(), 1);
        }
    }

    public async Task MigrateAsync()
    {
        Console.WriteLine("Start");
        await this.MigrateRegionAsync();
        Console.WriteLine("Region - 100%");
        await this.MigrateTenantsAsync();
        Console.WriteLine("Tenant - 100%");
        await this.MigrateCustomerUserProfileAsync();
        Console.WriteLine("CustomerUserProfile - 100%");
        //await MigrateOtherCustomerUserProfileAsync();
        await this.MigrateTicketStatusAsync();
        Console.WriteLine("TicketStatus - 100%");
        await this.MigrateTicketTypeAsync();
        Console.WriteLine("TicketType - 100%");
        for (var i = 0; i < 700; i++)
        {
            await this.MigrateTicketsAsync(10000, i * 10000);
        }
        await this.MigrateAutoIncrementFixAsync();
        Console.WriteLine("Ticket - 100%");
        await this.MigrateContractStatusAsync();
        Console.WriteLine("ContractStatus - 100%");
        await this.MigrateServicePackageAsync();
        Console.WriteLine("ServicePackage - 100%");
        await this.MigrateContractAndContractServiceAsync();
        Console.WriteLine("ContractAndContractService - 100%");
        await this.MigrateFileAsync(); // OK - 100% (blobstoring and avatar)
        Console.WriteLine("File - 100%");
        for (var i = 0; i < 600; i++)
        {
            await this.MigrateTicketAttachmentAsync(10000, i * 10000);
        }
        Console.WriteLine("TicketHistoryAttachment - 100%");
        for (var i = 0; i < 1500; i++)
        {
            await this.MigrateTicketHistoryAsync(8000, i * 8000);
        }
        Console.WriteLine("TicketHistory - 100%");
        for (var i = 10; i < 1500; i++)
        {
            await this.MigrateTicketHistoryAttachmentAsync(8000, i * 8000);
        }
        Console.WriteLine("TicketAttachment - 100%");
        Console.WriteLine("End");
    }
}
