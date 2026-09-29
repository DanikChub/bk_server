namespace KV.Server.Migration.Models;

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public partial class hdContentContext : DbContext
{
    public hdContentContext()
    {
    }

    public hdContentContext(DbContextOptions<hdContentContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ApplicationUserTicketWatcher> ApplicationUserTicketWatchers { get; set; }
    public virtual DbSet<ArchivedTicket> ArchivedTickets { get; set; }
    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }
    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }
    public virtual DbSet<AuthResource> AuthResources { get; set; }
    public virtual DbSet<ClassifierOkei> ClassifierOkeis { get; set; }
    public virtual DbSet<ClassifierOkof> ClassifierOkofs { get; set; }
    public virtual DbSet<ClassifierOkpd> ClassifierOkpds { get; set; }
    public virtual DbSet<ClassifierOktmo> ClassifierOktmos { get; set; }
    public virtual DbSet<ClassifierOkved> ClassifierOkveds { get; set; }
    public virtual DbSet<ConsultingLibPostType> ConsultingLibPostTypes { get; set; }
    public virtual DbSet<ConsultingLibraryCategory> ConsultingLibraryCategories { get; set; }
    public virtual DbSet<ConsultingLibraryPost> ConsultingLibraryPosts { get; set; }
    public virtual DbSet<Contract> Contracts { get; set; }
    public virtual DbSet<CustomerMediaLibraryMedia> CustomerMediaLibraryMedias { get; set; }
    public virtual DbSet<DirectMessage> DirectMessages { get; set; }
    public virtual DbSet<Hdcoment> Hdcoments { get; set; }
    public virtual DbSet<Hdprofile> Hdprofiles { get; set; }
    public virtual DbSet<HdservicePackage> HdservicePackages { get; set; }
    public virtual DbSet<HdservicePackageVendor> HdservicePackageVendors { get; set; }
    public virtual DbSet<Hdticket> Hdtickets { get; set; }
    public virtual DbSet<HdticketAttachment> HdticketAttachments { get; set; }
    public virtual DbSet<HdticketDocSpentHistory> HdticketDocSpentHistories { get; set; }
    public virtual DbSet<HdticketStatus> HdticketStatuses { get; set; }
    public virtual DbSet<HdticketStatusHistory> HdticketStatusHistories { get; set; }
    public virtual DbSet<MediaFile> MediaFiles { get; set; }
    public virtual DbSet<PressRelease> PressReleases { get; set; }
    public virtual DbSet<Region> Regions { get; set; }
    public virtual DbSet<SliderItem> SliderItems { get; set; }
    public virtual DbSet<UserEvent> UserEvents { get; set; }
    public virtual DbSet<UserEventVendor> UserEventVendors { get; set; }
    public virtual DbSet<UserNotification> UserNotifications { get; set; }
    public virtual DbSet<Vendor> Vendors { get; set; }
    public virtual DbSet<VendorDocSpentHistory> VendorDocSpentHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\Local;Database=hd_content;Trusted_Connection=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Cyrillic_General_CI_AS");

        modelBuilder.Entity<ApplicationUserTicketWatcher>(entity =>
        {
            entity.HasIndex(e => e.TicketId, "IX_ApplicationUserTicketWatchers_TicketId");

            entity.HasIndex(e => e.UserId, "IX_ApplicationUserTicketWatchers_UserId");

            entity.HasOne(d => d.Ticket)
                .WithMany(p => p.ApplicationUserTicketWatchers)
                .HasForeignKey(d => d.TicketId);

            entity.HasOne(d => d.User)
                .WithMany(p => p.ApplicationUserTicketWatchers)
                .HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ArchivedTicket>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("ArchivedTickets");

            entity.Property(e => e.AssignedToFullName).IsRequired();

            entity.Property(e => e.AssignedToUserName).HasMaxLength(256);

            entity.Property(e => e.AuthorFullName).IsRequired();

            entity.Property(e => e.AuthorUserName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);

            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.Property(e => e.RoleId).IsRequired();

            entity.HasOne(d => d.Role)
                .WithMany(p => p.AspNetRoleClaims)
                .HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);

            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles)
                .WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");

                        j.ToTable("AspNetUserRoles");

                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");

                        j.HasIndex(new[] { "UserId" }, "IX_AspNetUserRoles_UserId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.Property(e => e.UserId).IsRequired();

            entity.HasOne(d => d.User)
                .WithMany(p => p.AspNetUserClaims)
                .HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.UserId).IsRequired();

            entity.HasOne(d => d.User)
                .WithMany(p => p.AspNetUserLogins)
                .HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User)
                .WithMany(p => p.AspNetUserTokens)
                .HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AuthResource>(entity => entity.Property(e => e.Id).HasMaxLength(256));

        modelBuilder.Entity<ClassifierOkei>(entity =>
        {
            entity.ToTable("ClassifierOKEI");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Category).HasMaxLength(50);

            entity.Property(e => e.Code).HasMaxLength(256);

            entity.Property(e => e.DisplayCode).HasMaxLength(256);

            entity.Property(e => e.DisplayName).HasMaxLength(256);

            entity.Property(e => e.LeafNode).HasDefaultValueSql("((0))");

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__Classifie__Paren__33F4B129");
        });

        modelBuilder.Entity<ClassifierOkof>(entity =>
        {
            entity.ToTable("ClassifierOKOF");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Category).HasMaxLength(256);

            entity.Property(e => e.Code).HasMaxLength(256);

            entity.Property(e => e.DisplayCode).HasMaxLength(256);

            entity.Property(e => e.DisplayName).HasMaxLength(1024);
        });

        modelBuilder.Entity<ClassifierOkpd>(entity =>
        {
            entity.ToTable("ClassifierOKPD");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Category).HasMaxLength(256);

            entity.Property(e => e.Code).HasMaxLength(256);

            entity.Property(e => e.DisplayCode).HasMaxLength(256);

            entity.Property(e => e.DisplayName).HasMaxLength(1024);

            entity.Property(e => e.LeafNode).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<ClassifierOktmo>(entity =>
        {
            entity.ToTable("ClassifierOKTMO");

            entity.HasIndex(e => new { e.DisplayName, e.Code }, "IDX_ClassifierOKTMO2");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Category).HasMaxLength(200);

            entity.Property(e => e.Code).HasMaxLength(50);

            entity.Property(e => e.DisplayCode).HasMaxLength(50);

            entity.Property(e => e.DisplayName)
                .HasMaxLength(300)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ClassifierOkved>(entity =>
        {
            entity.ToTable("ClassifierOKVED");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Category).HasMaxLength(256);

            entity.Property(e => e.Code).HasMaxLength(256);

            entity.Property(e => e.DisplayCode).HasMaxLength(256);

            entity.Property(e => e.DisplayName).HasMaxLength(1024);
        });

        modelBuilder.Entity<ConsultingLibraryCategory>(entity => entity.HasMany(d => d.ConsultingLibraryPostsNavigation)
                .WithMany(p => p.ConsultingLibraryCategories)
                .UsingEntity<Dictionary<string, object>>(
                    "ConsultingLibraryPostConsultingLibraryCategory",
                    l => l.HasOne<ConsultingLibraryPost>().WithMany().HasForeignKey("ConsultingLibraryPostId").OnDelete(DeleteBehavior.ClientSetNull),
                    r => r.HasOne<ConsultingLibraryCategory>().WithMany().HasForeignKey("ConsultingLibraryCategoryId").OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("ConsultingLibraryCategoryId", "ConsultingLibraryPostId");

                        j.ToTable("ConsultingLibraryPostConsultingLibraryCategory");

                        j.HasIndex(new[] { "ConsultingLibraryPostId" }, "IX_ConsultingLibraryPostConsultingLibraryCategory_ConsultingLibraryPostId");
                    }));

        modelBuilder.Entity<ConsultingLibraryPost>(entity =>
        {
            entity.HasIndex(e => e.AuthorId, "IX_ConsultingLibraryPosts_AuthorId");

            entity.HasIndex(e => e.CategoryId, "IX_ConsultingLibraryPosts_CategoryId");

            entity.HasIndex(e => e.EditorId, "IX_ConsultingLibraryPosts_EditorId");

            entity.HasIndex(e => e.PostTypeId, "IX_ConsultingLibraryPosts_PostTypeId");

            entity.Property(e => e.Deleted).HasDefaultValueSql("((0))");

            entity.HasOne(d => d.Author)
                .WithMany(p => p.ConsultingLibraryPostAuthors)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Category)
                .WithMany(p => p.ConsultingLibraryPosts)
                .HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.Editor)
                .WithMany(p => p.ConsultingLibraryPostEditors)
                .HasForeignKey(d => d.EditorId);

            entity.HasOne(d => d.PostType)
                .WithMany(p => p.ConsultingLibraryPosts)
                .HasForeignKey(d => d.PostTypeId);

            entity.HasMany(d => d.Parents)
                .WithMany(p => p.Relateds)
                .UsingEntity<Dictionary<string, object>>(
                    "ConsultingLibraryPostRelated",
                    l => l.HasOne<ConsultingLibraryPost>().WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.ClientSetNull),
                    r => r.HasOne<ConsultingLibraryPost>().WithMany().HasForeignKey("RelatedId").OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("ParentId", "RelatedId");

                        j.ToTable("ConsultingLibraryPostRelateds");

                        j.HasIndex(new[] { "RelatedId" }, "IX_ConsultingLibraryPostRelateds_RelatedId");
                    });

            entity.HasMany(d => d.Relateds)
                .WithMany(p => p.Parents)
                .UsingEntity<Dictionary<string, object>>(
                    "ConsultingLibraryPostRelated",
                    l => l.HasOne<ConsultingLibraryPost>().WithMany().HasForeignKey("RelatedId").OnDelete(DeleteBehavior.ClientSetNull),
                    r => r.HasOne<ConsultingLibraryPost>().WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("ParentId", "RelatedId");

                        j.ToTable("ConsultingLibraryPostRelateds");

                        j.HasIndex(new[] { "RelatedId" }, "IX_ConsultingLibraryPostRelateds_RelatedId");
                    });
        });

        modelBuilder.Entity<OldModels.Contract>(entity =>
        {
            entity.HasIndex(e => e.ServicePackageId, "IX_Contracts_ServicePackageId");

            entity.HasIndex(e => e.VendorId, "IX_Contracts_VendorId");

            entity.HasOne(d => d.ServicePackage)
                .WithMany(p => p.Contracts)
                .HasForeignKey(d => d.ServicePackageId);

            entity.HasOne(d => d.Vendor)
                .WithMany(p => p.Contracts)
                .HasForeignKey(d => d.VendorId);
        });

        modelBuilder.Entity<CustomerMediaLibraryMedia>(entity =>
        {
            entity.HasIndex(e => e.AuthorId, "IX_CustomerMediaLibraryMedias_AuthorId");

            entity.HasIndex(e => e.FileContentId, "IX_CustomerMediaLibraryMedias_FileContentId");

            entity.HasOne(d => d.Author)
                .WithMany(p => p.CustomerMediaLibraryMedia)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.FileContent)
                .WithMany(p => p.CustomerMediaLibraryMedia)
                .HasForeignKey(d => d.FileContentId);
        });

        modelBuilder.Entity<DirectMessage>(entity =>
        {
            entity.HasIndex(e => e.AuthorId, "IX_DirectMessages_AuthorId");

            entity.HasIndex(e => e.RecipientId, "IX_DirectMessages_RecipientId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Author)
                .WithMany(p => p.DirectMessageAuthors)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Recipient)
                .WithMany(p => p.DirectMessageRecipients)
                .HasForeignKey(d => d.RecipientId);
        });

        modelBuilder.Entity<Hdcoment>(entity =>
        {
            entity.ToTable("HDComents");

            entity.HasIndex(e => e.AuthorId, "IX_HDComents_AuthorId");

            entity.HasIndex(e => e.HdticketId, "IX_HDComents_HDTicketId");

            entity.Property(e => e.HdticketId).HasColumnName("HDTicketId");

            entity.HasOne(d => d.Author)
                .WithMany(p => p.Hdcoments)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Hdticket)
                .WithMany(p => p.Hdcoments)
                .HasForeignKey(d => d.HdticketId);
        });

        modelBuilder.Entity<Hdprofile>(entity =>
        {
            entity.ToTable("HDProfiles");

            entity.HasIndex(e => e.UserId, "IX_HDProfiles_UserId");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Hdprofiles)
                .HasForeignKey(d => d.UserId);

            entity.HasOne(d => d.Vendor)
                .WithMany(p => p.Hdprofiles)
                .HasForeignKey(d => d.VendorId);
        });

        modelBuilder.Entity<HdservicePackage>(entity =>
        {
            entity.ToTable("HDServicePackages");

            entity.HasMany(d => d.Hdtickets)
                .WithMany(p => p.ServicePackages)
                .UsingEntity<Dictionary<string, object>>(
                    "HdservicePackageHdticket",
                    l => l.HasOne<Hdticket>().WithMany().HasForeignKey("HdticketId"),
                    r => r.HasOne<HdservicePackage>().WithMany().HasForeignKey("ServicePackageId"),
                    j =>
                    {
                        j.HasKey("ServicePackageId", "HdticketId");

                        j.ToTable("HDServicePackageHDTickets");

                        j.HasIndex(new[] { "HdticketId" }, "IX_HDServicePackageHDTickets_HDTicketId");

                        j.HasIndex(new[] { "ServicePackageId" }, "IX_HDServicePackageHDTickets_ServicePackageId");

                        j.IndexerProperty<int>("HdticketId").HasColumnName("HDTicketId");
                    });
        });

        modelBuilder.Entity<HdservicePackageVendor>(entity =>
        {
            entity.ToTable("HDServicePackageVendors");

            entity.HasIndex(e => e.ServicePackageId, "IX_HDServicePackageVendors_ServicePackageId");

            entity.HasIndex(e => e.VendorId, "IX_HDServicePackageVendors_VendorId");

            entity.HasOne(d => d.ServicePackage)
                .WithMany(p => p.HdservicePackageVendors)
                .HasForeignKey(d => d.ServicePackageId);

            entity.HasOne(d => d.Vendor)
                .WithMany(p => p.HdservicePackageVendors)
                .HasForeignKey(d => d.VendorId);
        });

        modelBuilder.Entity<Hdticket>(entity =>
        {
            entity.ToTable("HDTickets");

            entity.HasIndex(e => e.AssignedToId, "IX_HDTickets_AssignedToId");

            entity.HasIndex(e => e.StatusId, "IX_HDTickets_StatusId");

            entity.HasIndex(e => e.SupervisorId, "IX_HDTickets_SupervisorId");

            entity.HasOne(d => d.AssignedTo)
                .WithMany(p => p.HdticketAssignedTos)
                .HasForeignKey(d => d.AssignedToId);

            entity.HasOne(d => d.Author)
                .WithMany(p => p.Hdtickets)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Status)
                .WithMany(p => p.Hdtickets)
                .HasForeignKey(d => d.StatusId);

            entity.HasOne(d => d.Supervisor)
                .WithMany(p => p.HdticketSupervisors)
                .HasForeignKey(d => d.SupervisorId);
        });

        modelBuilder.Entity<HdticketAttachment>(entity =>
        {
            entity.ToTable("HDTicketAttachments");

            entity.HasIndex(e => e.AuthorId, "IX_HDTicketAttachments_AuthorId");

            entity.HasIndex(e => e.MediaFileInfoId, "IX_HDTicketAttachments_MediaFileInfoId");

            entity.HasIndex(e => e.TicketId, "IX_HDTicketAttachments_TicketId");

            entity.HasOne(d => d.Author)
                .WithMany(p => p.HdticketAttachments)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.MediaFileInfo)
                .WithMany(p => p.HdticketAttachments)
                .HasForeignKey(d => d.MediaFileInfoId);

            entity.HasOne(d => d.Ticket)
                .WithMany(p => p.HdticketAttachments)
                .HasForeignKey(d => d.TicketId);
        });

        modelBuilder.Entity<HdticketDocSpentHistory>(entity =>
        {
            entity.ToTable("HDTicketDocSpentHistory");

            entity.HasIndex(e => e.AuthorId, "IX_HDTicketDocSpentHistory_AuthorId");

            entity.HasIndex(e => e.TicketId, "IX_HDTicketDocSpentHistory_TicketId");

            entity.HasOne(d => d.Author)
                .WithMany(p => p.HdticketDocSpentHistories)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Ticket)
                .WithMany(p => p.HdticketDocSpentHistories)
                .HasForeignKey(d => d.TicketId);
        });

        modelBuilder.Entity<HdticketStatus>(entity => entity.ToTable("HDTicketStatuses"));

        modelBuilder.Entity<HdticketStatusHistory>(entity =>
        {
            entity.ToTable("HDTicketStatusHistory");

            entity.HasIndex(e => e.AuthorId, "IX_HDTicketStatusHistory_AuthorId");

            entity.HasIndex(e => e.StatusId, "IX_HDTicketStatusHistory_StatusId");

            entity.HasIndex(e => e.TicketId, "IX_HDTicketStatusHistory_TicketId");

            entity.HasOne(d => d.Author)
                .WithMany(p => p.HdticketStatusHistories)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Status)
                .WithMany(p => p.HdticketStatusHistories)
                .HasForeignKey(d => d.StatusId);

            entity.HasOne(d => d.Ticket)
                .WithMany(p => p.HdticketStatusHistories)
                .HasForeignKey(d => d.TicketId);
        });

        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.HasIndex(e => e.AuthorId, "IX_MediaFiles_AuthorId");

            entity.HasIndex(e => e.HdcomentId, "IX_MediaFiles_HDComentId");

            entity.HasIndex(e => e.HdticketStatusHistoryItemId, "IX_MediaFiles_HDTicketStatusHistoryItemId");

            entity.HasIndex(e => e.HdticketStatusHistoryItemId1, "IX_MediaFiles_HDTicketStatusHistoryItemId1")
                .IsUnique()
                .HasFilter("([HDTicketStatusHistoryItemId1] IS NOT NULL)");

            entity.Property(e => e.HdcomentId).HasColumnName("HDComentId");

            entity.Property(e => e.HdticketStatusHistoryItemId).HasColumnName("HDTicketStatusHistoryItemId");

            entity.Property(e => e.HdticketStatusHistoryItemId1).HasColumnName("HDTicketStatusHistoryItemId1");

            entity.HasOne(d => d.Author)
                .WithMany(p => p.MediaFiles)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Hdcoment)
                .WithMany(p => p.MediaFiles)
                .HasForeignKey(d => d.HdcomentId);

            entity.HasOne(d => d.HdticketStatusHistoryItem)
                .WithMany(p => p.MediaFileHdticketStatusHistoryItems)
                .HasForeignKey(d => d.HdticketStatusHistoryItemId);

            entity.HasOne(d => d.HdticketStatusHistoryItemId1Navigation)
                .WithOne(p => p.MediaFileHdticketStatusHistoryItemId1Navigation)
                .HasForeignKey<MediaFile>(d => d.HdticketStatusHistoryItemId1);
        });

        modelBuilder.Entity<PressRelease>(entity =>
        {
            entity.HasIndex(e => e.AuthorId, "IX_PressReleases_AuthorId");

            entity.HasIndex(e => e.RegionId, "IX_PressReleases_RegionId");

            entity.Property(e => e.Title).IsRequired();

            entity.HasOne(d => d.Author)
                .WithMany(p => p.PressReleases)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Region)
                .WithMany(p => p.PressReleases)
                .HasForeignKey(d => d.RegionId);
        });

        modelBuilder.Entity<SliderItem>(entity =>
        {
            entity.HasIndex(e => e.AuthorId, "IX_SliderItems_AuthorId");

            entity.HasIndex(e => e.ImageId, "IX_SliderItems_ImageId");

            entity.HasOne(d => d.Author)
                .WithMany(p => p.SliderItems)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Image)
                .WithMany(p => p.SliderItems)
                .HasForeignKey(d => d.ImageId);
        });

        modelBuilder.Entity<UserEvent>(entity =>
        {
            entity.HasIndex(e => e.AuthorId, "IX_UserEvents_AuthorId");

            entity.HasIndex(e => e.RecipientId, "IX_UserEvents_RecipientId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Title).IsRequired();

            entity.HasOne(d => d.Author)
                .WithMany(p => p.UserEventAuthors)
                .HasForeignKey(d => d.AuthorId);

            entity.HasOne(d => d.Recipient)
                .WithMany(p => p.UserEventRecipients)
                .HasForeignKey(d => d.RecipientId);
        });

        modelBuilder.Entity<UserEventVendor>(entity =>
        {
            entity.HasIndex(e => e.EventId, "IX_UserEventVendors_EventId");

            entity.HasIndex(e => e.VendorId, "IX_UserEventVendors_VendorId");

            entity.HasOne(d => d.Event)
                .WithMany(p => p.UserEventVendors)
                .HasForeignKey(d => d.EventId);

            entity.HasOne(d => d.Vendor)
                .WithMany(p => p.UserEventVendors)
                .HasForeignKey(d => d.VendorId);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_UserNotifications_UserId");

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserNotifications)
                .HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasIndex(e => e.RegionId, "IX_Vendors_RegionId");

            entity.Property(e => e.DirectorLogin)
                .IsRequired()
                .HasDefaultValueSql("(N'')");

            entity.Property(e => e.Innnumber).HasColumnName("INNNumber");

            entity.Property(e => e.Kppnumber).HasColumnName("KPPNumber");

            entity.HasOne(d => d.Region)
                .WithMany(p => p.Vendors)
                .HasForeignKey(d => d.RegionId);
        });

        modelBuilder.Entity<VendorDocSpentHistory>(entity =>
        {
            entity.HasKey(e => new { e.Year, e.Month, e.VendorId });

            entity.ToTable("VendorDocSpentHistory");

            entity.HasIndex(e => e.VendorId, "IX_VendorDocSpentHistory_VendorId");

            entity.HasOne(d => d.Vendor)
                .WithMany(p => p.VendorDocSpentHistories)
                .HasForeignKey(d => d.VendorId);
        });

        this.OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
