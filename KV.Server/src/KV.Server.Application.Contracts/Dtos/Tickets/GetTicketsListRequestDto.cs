namespace KV.Server.Dtos.Tickets;
using Volo.Abp.Application.Dtos;

public class GetTicketsListRequestDto : PagedAndSortedResultRequestDto
{
    public const int DefaultPageSize = 10;
    public string TicketStatusId { get; set; }
    public string ServicePackageId { get; set; }
    public string ManagerId { get; set; }
    public string SearchStr { get; set; }

    public string IdentityUserId { get; set; }
    public string ResponsibleId { get; set; }
    public string TenantId { get; set; }
    public string SearchTicketId { get; set; }
    public string SearchSubject { get; set; }
    public string SearchTicketStatusId { get; set; }
    public string SearchTicketTypeId { get; set; }
    public string SearchTagId { get; set; }
    public string SearchTicketSectionId { get; set; }
    public string SearchCreatorFullName { get; set; }
    public string SearchResponsibleFullName { get; set; }
    public string SearchCustomerShortName { get; set; }
    public string SearchCustomerLongName { get; set; }
    public string SearchCreationTime { get; set; }
    public string SearchDueDate { get; set; }
}
