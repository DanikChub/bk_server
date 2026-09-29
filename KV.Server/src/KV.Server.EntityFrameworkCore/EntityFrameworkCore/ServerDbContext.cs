namespace KV.Server.EntityFrameworkCore;

using System;
using KV.Server.Contracts;
using KV.Server.Events;
using KV.Server.File;
using KV.Server.Notifications;
using KV.Server.Profiles;
using KV.Server.Tags;
using KV.Server.Templates;
using KV.Server.Tenants;
using KV.Server.TicketHoursSpent;
using KV.Server.Tickets;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class ServerDbContext :
    AbpDbContext<ServerDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules
    //DbSet сущностей профилей тенантов
    public DbSet<TenantProfile> TenantProfiles { get; set; }
    //DbSet сущностей профилей тенантов и пользователей (рабочий по компание)
    public DbSet<TenantProfileManager> TenantProfileManagers { get; set; }
    ////DbSet сущностей историй
    public DbSet<StoryItem> Stories { get; set; }
    //DbSet сущностей слайдов историй
    public DbSet<SlideItem> Slides { get; set; }

    //DbSet сущностей заявки
    public DbSet<Ticket> Tickets { get; set; }
    //DbSet сущностей пользователей
    public DbSet<CustomerUserProfile> CustomerUserProfiles { get; set; }

    public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }
    //DbSet сущностей заявки вложенностей
    public DbSet<TicketAttachment> TicketAttachments { get; set; }
    //DbSet сущностей истории заявки вложенностей
    public DbSet<TicketHistoryAttachment> TicketHistoryAttachments { get; set; }
    //DbSet сущностей заявки истории
    public DbSet<TicketHistory> TicketHistory { get; set; }
    //DbSet сущностей заявки статусов
    public DbSet<TicketStatus> TicketStatuses { get; set; }
    //DbSet сущностей заявки типов
    public DbSet<TicketType> TicketTypes { get; set; }
    //DbSet сущностей заявки раздел
    public DbSet<TicketSection> TicketSections { get; set; }
    //DbSet сущностей комментарии под заявкой
    public DbSet<TicketMessage> TicketMessages { get; set; }
    //DbSet сущностей истории списание часов
    public DbSet<TicketHoursSpentHistory> TicketHoursSpentHistories { get; set; }
    //DbSet сущностей избранных заявок для клиента
    public DbSet<TicketClientFavorite> TicketClientFavorites { get; set; }
    //DbSet сущностей избранных заявок для специалиста
    public DbSet<TicketSpecialistFavorite> TicketSpecialistFavorites { get; set; }
    //DbSet сущностей истории списание часов
    public DbSet<ConstraintType> ConstraintTypes { get; set; }
    //DbSet сущностей теги и заявки
    public DbSet<TicketTag> TicketTags { get; set; }
    //DbSet сущностей файла
    public DbSet<UploadedFile> UploadedFiles { get; set; }
    //DbSet сущностей шаблон ответа
    public DbSet<AnswerTemplate> AnswerTemplates { get; set; }
    //DbSet сущностей теги
    public DbSet<Tag> Tags { get; set; }
    //DbSet сущностей событий
    public DbSet<CalendarEvent> Events { get; set; }
    //DbSet сущностей ограничений за месяц
    public DbSet<ContractConstraintsByMonth> ContractConstraintsByMonths { get; set; }
    //DbSet сущностей разделов за месяц
    public DbSet<ContractTicketsByTicketSectionByMonth> ContractTicketsByTicketSectionByMonths { get; set; }
    //DbSet сущностей типов заявок за месяц
    public DbSet<ContractTicketsByTicketTypeByMonth> ContractTicketsByTicketTypeByMonths { get; set; }

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractSetting> ContractSettings { get; set; }
    public DbSet<ContractTag> ContractTags { get; set; }
    public DbSet<ContractStatus> ContractStatuses { get; set; }
    public DbSet<ServicePackage> ServicePackages { get; set; }
    public DbSet<ContractServicePackage> ContractServicePackages { get; set; }

    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }

    public DbSet<UserNotification> UserNotifications { get; set; }
    public DbSet<NotificationCategory> NotificationCategories { get; set; }
    public DbSet<NotificationStatus> NotificationStatuses { get; set; }
    #endregion

    public ServerDbContext(DbContextOptions<ServerDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        /* Include modules to your migration db context */

        modelBuilder.ConfigurePermissionManagement();
        modelBuilder.ConfigureSettingManagement();
        modelBuilder.ConfigureBackgroundJobs();
        modelBuilder.ConfigureAuditLogging();
        modelBuilder.ConfigureIdentity();
        modelBuilder.ConfigureOpenIddict();
        modelBuilder.ConfigureFeatureManagement();
        modelBuilder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        modelBuilder.Entity<TenantProfile>(b =>
        {
            b.ToTable("TenantProfiles");
            b.HasOne(x => x.ResponsibleManager).WithMany().HasForeignKey(x => x.ResponsibleManagerId);
        });
        modelBuilder.Entity<Region>(b =>
        {
            b.ToTable("Regions");
            b.HasMany(p => p.TenantsProfile).WithOne(x => x.Region).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<StoryItem>(b => b.ToTable("Stories"));
        modelBuilder.Entity<SlideItem>(b =>
        {
            b.ToTable("Slides");
            b.HasOne<StoryItem>().WithMany().HasForeignKey(x => x.StoryId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ServicePackage>(b => b.ToTable("ServicePackages"));
        modelBuilder.Entity<ContractServicePackage>(b =>
        {
            b.ToTable("ContractServicePackages");
            b.HasKey(c => new { c.ContractId, c.ServicePackageId });
        });
        modelBuilder.Entity<Contract>(x =>
        {
            x.ToTable("Contracts");
            x.HasMany(p => p.ContractSettings).WithOne(x => x.Contract).HasForeignKey(p => p.ContractId).OnDelete(DeleteBehavior.Cascade);
            x.HasOne(p => p.TenantProfile).WithMany(x => x.Contracts).HasForeignKey(p => p.TenantId);
            x.Property(x => x.ServicePackagesData).ValueGeneratedOnAddOrUpdate();
        });
        modelBuilder.Entity<ContractStatus>(x =>
        {
            x.ToTable("ContractStatuses");
            x.HasIndex(x => x.Title);
            x.HasIndex(x => x.Code);
        });

        modelBuilder.Entity<UploadedFile>(x => x.ToTable("UploadedFiles"));
        modelBuilder.Entity<UserNotification>(x =>
        {
            x.ToTable("UserNotifications");
            x.HasOne(x => x.NotificationCategory).WithMany(x => x.UserNotifications).HasForeignKey(x => x.NotificationCategoryId);
            x.HasOne(x => x.IdentityUser).WithMany().HasForeignKey(x => x.IdentityUserId);
            x.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatorId);
        });
        modelBuilder.Entity<CustomerUserProfile>(x =>
        {
            x.ToTable("CustomerUserProfiles");
            x.HasOne(x => x.TenantProfile).WithMany(x => x.CustomerUserProfiles).HasForeignKey(x => x.TenantId);
            x.HasOne(x => x.UserAvatarFile).WithMany();
        });
        modelBuilder.Entity<EmployeeProfile>(x =>
        {
            x.ToTable("EmployeeProfiles");
            x.HasOne(x => x.UserAvatarFile).WithMany();
        });
        modelBuilder.Entity<ContractTag>(x =>
        {
            x.ToTable("ContractTags");
            x.HasKey(c => new { c.ContractId, c.TagId });
        });
        modelBuilder.Entity<TicketTag>(x =>
        {
            x.ToTable("TicketTags");
            x.HasKey(c => new { c.TicketId, c.TagId });
        });
        modelBuilder.Entity<TicketAttachment>(b =>
        {
            b.ToTable("TicketAttachments");
            b.HasKey(c => new { c.TicketId, c.UploadedFileId });
        });
        modelBuilder.Entity<TicketHistoryAttachment>(b =>
        {
            b.ToTable("TicketHistoryAttachments");
            b.HasKey(c => new { c.TicketHistoryId, c.UploadedFileId });
        });
        modelBuilder.Entity<TenantProfileManager>(b =>
        {
            b.ToTable("TenantProfileManagers");
            b.HasKey(c => new { c.TenantProfileId, c.ManagerId });
        });
        modelBuilder.Entity<TicketHistory>(b =>
        {
            b.ToTable("TicketHistory");
            b.HasMany(x => x.TicketHistoryAttachments).WithOne(x => x.TicketHistory).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.TicketStatus).WithMany(x => x.TicketHistories);
        });
        modelBuilder.Entity<TicketStatus>(b => b.ToTable("TicketStatuses"));
        modelBuilder.Entity<TicketType>(b => b.ToTable("TicketTypes"));
        modelBuilder.Entity<Ticket>(x =>
        {
            x.ToTable("Tickets");
            x.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatorId);
            x.HasOne(x => x.Responsible).WithMany(x => x.ResponsibleTickets).HasForeignKey(x => x.ResponsibleId);
            x.HasOne(x => x.TicketType).WithMany(x => x.Tickets);
            x.HasOne(x => x.TicketStatus).WithMany(x => x.Tickets);
            x.HasOne(x => x.TenantProfile).WithMany(x => x.Tickets).HasForeignKey(x => x.TenantId);
            x.HasOne(x => x.Contract).WithMany(x => x.Tickets).HasForeignKey(x => x.ContractId);
            x.HasOne(x => x.TicketSection).WithMany(x => x.Tickets).HasForeignKey(x => x.TicketSectionId);
            x.HasMany(x => x.TicketTags).WithOne(x => x.Ticket).OnDelete(DeleteBehavior.Cascade);
            x.HasMany(x => x.TicketAttachments).WithOne(x => x.Ticket).OnDelete(DeleteBehavior.Cascade);
            x.HasMany(x => x.TicketHistory).WithOne(x => x.Ticket).OnDelete(DeleteBehavior.Cascade);
            x.HasMany(x => x.Events).WithOne(x => x.Ticket).OnDelete(DeleteBehavior.Cascade);
            x.Property(x => x.ServicePackages).ValueGeneratedOnAddOrUpdate();
        });
        modelBuilder.Entity<TicketMessage>(x =>
        {
            x.ToTable("TicketMessages");
            x.HasOne(x => x.Ticket).WithMany(x => x.TicketMessages).HasForeignKey(x => x.TicketId);
            x.HasOne(x => x.CreatorCustomerUserProfile).WithMany(x => x.TicketCreatorMessages).HasForeignKey(x => x.CreatorCustomerUserProfileId);
        });
        modelBuilder.Entity<TicketHoursSpentHistory>(x =>
        {
            x.ToTable("TicketHoursSpentHistories");
            x.HasOne(x => x.TicketHistory).WithOne(x => x.TicketHoursSpentHistory).HasForeignKey<TicketHoursSpentHistory>(x => x.TicketHistoryId);
            x.HasOne(x => x.ConstraintType).WithMany(x => x.TicketHoursSpentHistories).HasForeignKey(x => x.ConstraintTypeId);
        });
        modelBuilder.Entity<AnswerTemplate>(x => x.ToTable("AnswerTemplates"));
        modelBuilder.Entity<CalendarEvent>(x => x.ToTable("Events"));
        modelBuilder.Entity<TicketClientFavorite>(x =>
        {
            x.ToTable("TicketClientFavorites");
            x.HasKey(c => new { c.ClientId, c.TicketId });
        });
        modelBuilder.Entity<TicketSpecialistFavorite>(x =>
        {
            x.ToTable("TicketSpecialistFavorites");
            x.HasKey(c => new { c.SpecialistId, c.TicketId });
        });
        modelBuilder.Entity<ContractTicketsByTicketTypeByMonth>(b =>
        {
            b.ToTable("ContractTicketsByTicketTypeByMonths");
            b.HasOne(x => x.Contract).WithMany();
            b.HasOne(x => x.TicketType).WithMany();
        });
        modelBuilder.Entity<ContractTicketsByTicketSectionByMonth>(b =>
        {
            b.ToTable("ContractTicketsByTicketSectionByMonths");
            b.HasOne(x => x.Contract).WithMany();
            b.HasOne(x => x.TicketSection).WithMany();
        });
        modelBuilder.Entity<ContractConstraintsByMonth>(b =>
        {
            b.ToTable("ContractConstraintsByMonths");
            b.HasOne(x => x.Contract).WithMany();
            b.HasOne(x => x.ConstraintType).WithMany();
            b.HasIndex(x => new {
                x.Year,
                x.Month,
                x.ConstraintTypeId,
                x.ContractId
            }, "IX_ContractConstraintsByMonths_Year_Month_ConstraintTypeId")
                .IsUnique(true);
        });
        modelBuilder.Entity<ConstraintType>().HasData(
            new ConstraintType(new Guid("5575ad4c-45c3-4bfb-88c1-8d0e3ff6ae65"), "НПА"),
            new ConstraintType(new Guid("5ef7dd65-e63f-4d94-a270-4a2d4c74728e"), "Закупки"),
            new ConstraintType(new Guid("63aa36f1-5185-4d77-9fd8-02f78c062808"), "Торги"),
            new ConstraintType(new Guid("6d562221-4437-48d7-b17c-ba4c21328a86"), "БУХУЧЕТ"),
            new ConstraintType(new Guid("8b1fe2cb-165c-449d-a005-71870f37085b"), "Претензии"));

        modelBuilder.Entity<TicketType>().HasData(
            new TicketType(new Guid("a736e1b2-3ad6-471f-971f-d2a27f86a417"), "Письменная", new TimeSpan(2, 0, 0, 0)),
            new TicketType(new Guid("a736e1b2-3ad6-471f-971f-d2a27f86a416"), "Устная", new TimeSpan(2, 0, 0, 0)),
            new TicketType(new Guid("1736e1b2-3ad6-471f-971f-d2a27f86a416"), "Размещение", new TimeSpan(7, 0, 0, 0)),
            new TicketType(new Guid("2736e1b2-3ad6-471f-971f-d2a27f86a416"), "Разработка", new TimeSpan(7, 0, 0, 0)),
            new TicketType(new Guid("3736e1b2-3ad6-471f-971f-d2a27f86a416"), "Сайт", new TimeSpan(2, 0, 0, 0)),
            new TicketType(new Guid("4736e1b2-3ad6-471f-971f-d2a27f86a416"), "ИТ", new TimeSpan(2, 0, 0, 0)));

        modelBuilder.Entity<TicketSection>().HasData(
            new TicketSection(new Guid("1736e1b2-3ad6-471f-971f-d2a27f86a417"), "Бухгалтерский учет"),
            new TicketSection(new Guid("2736e1b2-3ad6-471f-971f-d2a27f86a417"), "Гражданское законодательство"),
            new TicketSection(new Guid("3736e1b2-3ad6-471f-971f-d2a27f86a417"), "Административное законодательство"),
            new TicketSection(new Guid("4736e1b2-3ad6-471f-971f-d2a27f86a417"), "Бюджетное законодательство"),
            new TicketSection(new Guid("5736e1b2-3ad6-471f-971f-d2a27f86a417"), "Полномочия органов власти(ОМСУ)"),
            new TicketSection(new Guid("6736e1b2-3ad6-471f-971f-d2a27f86a417"), "Трудовое законодательство"),
            new TicketSection(new Guid("7736e1b2-3ad6-471f-971f-d2a27f86a417"), "Земельное законодательство"),
            new TicketSection(new Guid("8736e1b2-3ad6-471f-971f-d2a27f86a417"), "Муниципальное имущество"),
            new TicketSection(new Guid("9736e1b2-3ad6-471f-971f-d2a27f86a417"), "Закупки(44 - ФЗ, 223 - ФЗ)"),
            new TicketSection(new Guid("a036e1b2-3ad6-471f-971f-d2a27f86a417"), "Налоговое законодательство"),
            new TicketSection(new Guid("b136e1b2-3ad6-471f-971f-d2a27f86a417"), "Противодействие коррупции"),
            new TicketSection(new Guid("c236e1b2-3ad6-471f-971f-d2a27f86a417"), "Гражданский / Арбитражный процесс, КАС"),
            new TicketSection(new Guid("d336e1b2-3ad6-471f-971f-d2a27f86a417"), "Торги(Аренда, Продажа, Отбор УК)"),
            new TicketSection(new Guid("e736e1b2-3ad6-471f-971f-d2a27f86a417"), "Антимонопольное законодательство"),
            new TicketSection(new Guid("f736e1b2-3ad6-471f-971f-d2a27f86a417"), "Гражданская / Муниципальная служба"),
            new TicketSection(new Guid("fa36e1b2-3ad6-471f-971f-d2a27f86a417"), "НТО"),
            new TicketSection(new Guid("fb36e1b2-3ad6-471f-971f-d2a27f86a417"), "Корпоративное право"),
            new TicketSection(new Guid("fc36e1b2-3ad6-471f-971f-d2a27f86a417"), "Иное"));
    }
}
