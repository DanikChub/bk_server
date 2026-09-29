namespace KV.Server;
using System;
using System.Collections.Generic;

public class CreateUpdateDetailsContractDto
{
    public Guid? ServicePackageId { get; set; }
    public List<TagDto> Tags { get; set; }
    public List<ConstraintTypeDto> ConstraintTypes { get; set; }
}
