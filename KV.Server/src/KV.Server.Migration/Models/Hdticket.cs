namespace KV.Server.Migration.Models;

using System;
using System.Collections.Generic;

public partial class Hdticket
{
    public Hdticket()
    {
        this.ApplicationUserTicketWatchers = new HashSet<ApplicationUserTicketWatcher>();
        this.Hdcoments = new HashSet<Hdcoment>();
        this.HdticketAttachments = new HashSet<HdticketAttachment>();
        this.HdticketDocSpentHistories = new HashSet<HdticketDocSpentHistory>();
        this.HdticketStatusHistories = new HashSet<HdticketStatusHistory>();
        this.ServicePackages = new HashSet<HdservicePackage>();
    }

    public int Id { get; set; }
    public string AssignedToId { get; set; }
    public int? AuthorId { get; set; }
    public string Body { get; set; }
    public DateTime Created { get; set; }
    public DateTime DueDate { get; set; }
    public int? StatusId { get; set; }
    public string Subject { get; set; }
    public string SupervisorId { get; set; }
    public DateTime Updated { get; set; }
    public double Rating { get; set; }

    public virtual AspNetUser AssignedTo { get; set; }
    public virtual Hdprofile Author { get; set; }
    public virtual HdticketStatus Status { get; set; }
    public virtual AspNetUser Supervisor { get; set; }
    public virtual ICollection<ApplicationUserTicketWatcher> ApplicationUserTicketWatchers { get; set; }
    public virtual ICollection<Hdcoment> Hdcoments { get; set; }
    public virtual ICollection<HdticketAttachment> HdticketAttachments { get; set; }
    public virtual ICollection<HdticketDocSpentHistory> HdticketDocSpentHistories { get; set; }
    public virtual ICollection<HdticketStatusHistory> HdticketStatusHistories { get; set; }

    public virtual ICollection<HdservicePackage> ServicePackages { get; set; }
}
