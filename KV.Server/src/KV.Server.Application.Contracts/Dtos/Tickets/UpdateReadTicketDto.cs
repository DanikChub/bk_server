namespace KV.Server;
using System;

public class UpdateReadTicketDto
{
    public DateTime? ReadByClientDate { get; set; }
    public DateTime? ReadBySpecialistDate { get; set; }
}
