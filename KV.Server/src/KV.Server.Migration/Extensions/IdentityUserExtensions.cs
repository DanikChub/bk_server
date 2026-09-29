namespace KV.Server.Migration.Extensions;

using Volo.Abp.Identity;

public static class IdentityUserExtensions
{
    public static void SetPasswordHash(this IdentityUser identityUser, string PasswordHash)
    {
        var info = typeof(IdentityUser).GetProperty("PasswordHash");
        info?.SetValue(identityUser, PasswordHash);
    }
}
