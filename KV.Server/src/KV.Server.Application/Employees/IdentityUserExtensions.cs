namespace KV.Server.Employees;
using System;
using Volo.Abp.Identity;

public static class IdentityUserExtensions
{
    public static void SetTenantId(this IdentityUser identityUser, Guid? tenantId)
    {
        var identityUserInfo = typeof(IdentityUser).GetProperty("TenantId");
        identityUserInfo.SetValue(identityUser, tenantId);
    }
}
