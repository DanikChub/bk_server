namespace KV.Server.Migration.Extensions;

using System;

public static class ContractStatusExtensions
{
    public static void SetId(this ContractStatus contractStatus, Guid Id)
    {
        var info = typeof(ContractStatus).GetProperty("Id");
        info?.SetValue(contractStatus, Id);
    }
}
