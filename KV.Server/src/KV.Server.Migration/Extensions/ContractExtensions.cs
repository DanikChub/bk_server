namespace KV.Server.Migration.Extensions;

using System;

public static class ContractExtensions
{
    public static void SetCreationTime(this Contract contract, DateTime CreationTime)
    {
        var info = typeof(Contract).GetProperty("CreationTime");
        info?.SetValue(contract, CreationTime);
    }

    public static void SetCreatorId(this Contract contract, Guid? CreatorId)
    {
        var info = typeof(Contract).GetProperty("CreatorId");
        info?.SetValue(contract, CreatorId);
    }
}
