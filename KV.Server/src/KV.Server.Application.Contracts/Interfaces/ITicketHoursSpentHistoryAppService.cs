namespace KV.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface ITicketHoursSpentHistoryAppService : IApplicationService
{
    Task<TicketHoursSpentHistoryDto> SetSpentTimeAsync(CreateUpdateTicketHoursSpentHistoryDto createDto);
    Task<List<ConstraintTypeContractDto>> ToListDisplayConstraintTypesContractId(Guid? contractId);
    Task<List<ConstraintTypeDto>> ToListTicketRangeByTicketIdAsync(long ticketId);
    Task<List<ConstraintTypeDto>> ToListConstraintTypesAsync();
}
