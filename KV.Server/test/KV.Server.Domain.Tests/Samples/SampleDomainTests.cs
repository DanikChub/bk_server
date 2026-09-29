namespace KV.Server.Samples;

using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Identity;
using Xunit;

/* This is just an example test class.
 * Normally, you don't test code of the modules you are using
 * (like IdentityUserManager here).
 * Only test your own domain services.
 */
public class SampleDomainTests : ServerDomainTestBase
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;

    public SampleDomainTests()
    {
        this._identityUserRepository = this.GetRequiredService<IIdentityUserRepository>();
        this._identityUserManager = this.GetRequiredService<IdentityUserManager>();
    }

    [Fact]
    public async Task ShouldSetEmailOfAUser()
    {
        IdentityUser adminUser;

        /* Need to manually start Unit Of Work because
         * FirstOrDefaultAsync should be executed while db connection / context is available.
         */
        await this.WithUnitOfWorkAsync(async () =>
        {
            adminUser = await this._identityUserRepository
                .FindByNormalizedUserNameAsync("ADMIN");

            await this._identityUserManager.SetEmailAsync(adminUser, "newemail@abp.io");
            await this._identityUserRepository.UpdateAsync(adminUser);
        });

        adminUser = await this._identityUserRepository.FindByNormalizedUserNameAsync("ADMIN");
        adminUser.Email.ShouldBe("newemail@abp.io");
    }
}
