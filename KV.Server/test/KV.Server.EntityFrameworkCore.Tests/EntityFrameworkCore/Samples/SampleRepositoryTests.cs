namespace KV.Server.EntityFrameworkCore.Samples;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Xunit;

/* This is just an example test class.
 * Normally, you don't test ABP framework code
 * (like default AppUser repository IRepository<AppUser, Guid> here).
 * Only test your custom repository methods.
 */
public class SampleRepositoryTests : ServerEntityFrameworkCoreTestBase
{
    private readonly IRepository<IdentityUser, Guid> _appUserRepository;

    public SampleRepositoryTests()
    {
        this._appUserRepository = this.GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task ShouldQueryAppUser() =>
        /* Need to manually start Unit Of Work because
* FirstOrDefaultAsync should be executed while db connection / context is available.
*/
        await this.WithUnitOfWorkAsync(async () =>
        {
            //Act
            var adminUser = await (await this._appUserRepository.GetQueryableAsync())
                .Where(u => u.UserName == "admin")
                .FirstOrDefaultAsync();

            //Assert
            adminUser.ShouldNotBeNull();
        });
}
