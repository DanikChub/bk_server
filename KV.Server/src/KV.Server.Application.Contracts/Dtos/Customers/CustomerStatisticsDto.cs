namespace KV.Server;
using System.Collections.Generic;

public class CustomerStatisticsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<ContractConstraintsByMonthDto> ConstraintsByMonths { get; set; } = new();
    public List<ContractTicketsByTicketSectionByMonthDto> TicketSectionsByMonths { get; set; } = new();
    public List<ContractTicketsByTicketTypeByMonthDto> TicketTypesByMonths { get; set; } = new();
}
