namespace KV.Server.Security;

using System.Collections.Generic;
using System.Security.Claims;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

[Dependency(ReplaceServices = true)]
public class FakeCurrentPrincipalAccessor : ThreadCurrentPrincipalAccessor
{
    protected override ClaimsPrincipal GetClaimsPrincipal() => this.GetPrincipal();

    private ClaimsPrincipal _principal;

    private ClaimsPrincipal GetPrincipal()
    {
        if (this._principal == null)
        {
            lock (this)
            {
                this._principal ??= new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new List<Claim>
                            {
                                new Claim(AbpClaimTypes.UserId,"2e701e62-0953-4dd3-910b-dc6cc93ccb0d"),
                                new Claim(AbpClaimTypes.UserName,"admin"),
                                new Claim(AbpClaimTypes.Email,"admin@abp.io")
                            }
                        )
                    );
            }
        }

        return this._principal;
    }
}
