namespace KV.Server.Pages;

using System.Threading.Tasks;
using Shouldly;
using Xunit;

public class IndexTests : ServerWebTestBase
{
    [Fact]
    public async Task WelcomePage()
    {
        var response = await this.GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
