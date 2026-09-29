namespace KV.Server.Samples;

using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Identity;
using Xunit;

/* This is just an example test class.
 * Normally, you don't test code of the modules you are using
 * (like IIdentityUserAppService here).
 * Only test your own application services.
 */
public class SampleAppServiceTests : ServerApplicationTestBase
{
    private readonly IIdentityUserAppService _userAppService;

    public SampleAppServiceTests()
    {
        this._userAppService = this.GetRequiredService<IIdentityUserAppService>();
    }

    [Fact]
    public async Task InitialDataShouldContainAdminUser()
    {
        //Act
        var result = await this._userAppService.GetListAsync(new GetIdentityUsersInput());

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(u => u.UserName == "admin");
    }
}
