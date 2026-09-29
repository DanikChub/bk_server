namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class AspNetUser
{
    public AspNetUser()
    {
        this.ApplicationUserTicketWatchers = new HashSet<ApplicationUserTicketWatcher>();
        this.AspNetUserClaims = new HashSet<AspNetUserClaim>();
        this.AspNetUserLogins = new HashSet<AspNetUserLogin>();
        this.AspNetUserTokens = new HashSet<AspNetUserToken>();
        this.ConsultingLibraryPostAuthors = new HashSet<ConsultingLibraryPost>();
        this.ConsultingLibraryPostEditors = new HashSet<ConsultingLibraryPost>();
        this.CustomerMediaLibraryMedia = new HashSet<CustomerMediaLibraryMedia>();
        this.DirectMessageAuthors = new HashSet<DirectMessage>();
        this.DirectMessageRecipients = new HashSet<DirectMessage>();
        this.Hdcoments = new HashSet<Hdcoment>();
        this.Hdprofiles = new HashSet<Hdprofile>();
        this.HdticketAssignedTos = new HashSet<Hdticket>();
        this.HdticketAttachments = new HashSet<HdticketAttachment>();
        this.HdticketDocSpentHistories = new HashSet<HdticketDocSpentHistory>();
        this.HdticketStatusHistories = new HashSet<HdticketStatusHistory>();
        this.HdticketSupervisors = new HashSet<Hdticket>();
        this.MediaFiles = new HashSet<MediaFile>();
        this.PressReleases = new HashSet<PressRelease>();
        this.SliderItems = new HashSet<SliderItem>();
        this.UserEventAuthors = new HashSet<UserEvent>();
        this.UserEventRecipients = new HashSet<UserEvent>();
        this.UserNotifications = new HashSet<UserNotification>();
        this.Roles = new HashSet<AspNetRole>();
    }

    public string Id { get; set; }
    public int AccessFailedCount { get; set; }
    public string ConcurrencyStamp { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public string MiddleName { get; set; }
    public string NormalizedEmail { get; set; }
    public string NormalizedUserName { get; set; }
    public string PasswordHash { get; set; }
    public string PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string SecurityStamp { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string UserName { get; set; }
    public bool IsDisabled { get; set; }
    public string AvatarUrl { get; set; }
    public string Department { get; set; }
    public string Subtitle { get; set; }

    public virtual ICollection<ApplicationUserTicketWatcher> ApplicationUserTicketWatchers { get; set; }
    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; }
    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; }
    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; }
    public virtual ICollection<ConsultingLibraryPost> ConsultingLibraryPostAuthors { get; set; }
    public virtual ICollection<ConsultingLibraryPost> ConsultingLibraryPostEditors { get; set; }
    public virtual ICollection<CustomerMediaLibraryMedia> CustomerMediaLibraryMedia { get; set; }
    public virtual ICollection<DirectMessage> DirectMessageAuthors { get; set; }
    public virtual ICollection<DirectMessage> DirectMessageRecipients { get; set; }
    public virtual ICollection<Hdcoment> Hdcoments { get; set; }
    public virtual ICollection<Hdprofile> Hdprofiles { get; set; }
    public virtual ICollection<Hdticket> HdticketAssignedTos { get; set; }
    public virtual ICollection<HdticketAttachment> HdticketAttachments { get; set; }
    public virtual ICollection<HdticketDocSpentHistory> HdticketDocSpentHistories { get; set; }
    public virtual ICollection<HdticketStatusHistory> HdticketStatusHistories { get; set; }
    public virtual ICollection<Hdticket> HdticketSupervisors { get; set; }
    public virtual ICollection<MediaFile> MediaFiles { get; set; }
    public virtual ICollection<PressRelease> PressReleases { get; set; }
    public virtual ICollection<SliderItem> SliderItems { get; set; }
    public virtual ICollection<UserEvent> UserEventAuthors { get; set; }
    public virtual ICollection<UserEvent> UserEventRecipients { get; set; }
    public virtual ICollection<UserNotification> UserNotifications { get; set; }

    public virtual ICollection<AspNetRole> Roles { get; set; }
}
