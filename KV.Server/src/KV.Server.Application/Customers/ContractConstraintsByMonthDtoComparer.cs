namespace KV.Server;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

internal class ContractConstraintsByMonthDtoComparer : IEqualityComparer<ContractConstraintsByMonthDto>
{
    public bool Equals(ContractConstraintsByMonthDto x, ContractConstraintsByMonthDto y)
    {
        return x.Name == y.Name;
    }

    public int GetHashCode([DisallowNull] ContractConstraintsByMonthDto obj)
    {
        return $"{obj.Name}".GetHashCode();
    }
}
