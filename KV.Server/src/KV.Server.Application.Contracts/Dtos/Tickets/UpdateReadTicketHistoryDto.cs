namespace KV.Server;
using System;

public class UpdateReadTicketHistoryDto
{
    public DateTime? ReadByClientDate { get; set; }
    public DateTime? ReadBySpecialistDate { get; set; }
}
