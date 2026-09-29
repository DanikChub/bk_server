namespace KV.Server;
using System.Collections.Generic;

public class CustomerContractStatisticsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<ContractConstraintsByMonthDto> ConstraintsByMonths { get; set; } = new();
}
