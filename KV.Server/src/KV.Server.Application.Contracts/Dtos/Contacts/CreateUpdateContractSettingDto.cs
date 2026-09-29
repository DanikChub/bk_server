namespace KV.Server;

using System;

public class CreateUpdateContractSettingDto
{
    public int Max { get; set; }
    public Guid ConstraintTypeId { get; set; }
}
