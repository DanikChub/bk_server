namespace KV.Server.Dtos.Tickets;
using System;
using Volo.Abp.Application.Dtos;

public class TicketResponsibleCountDto : EntityDto<Guid>
{
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public int CountTicketTake { get; set; }
}
