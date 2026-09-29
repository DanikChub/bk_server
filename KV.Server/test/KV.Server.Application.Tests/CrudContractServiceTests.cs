namespace KV.Server;

using System;
using System.Threading.Tasks;
using Xunit;

public class CrudContractServiceTests : ServerApplicationTestBase
{
    private readonly CrudContractService _crudContractService;

    public CrudContractServiceTests()
    {
        this._crudContractService = this.GetRequiredService<CrudContractService>();
    }

    [Fact]
    public async Task AddContract()
    {
        var item = await this._crudContractService.CreateAsync(new CreateUpdateContractDto
        {
            TenantId = new Guid("3a04313a-2c19-b621-cff6-6c8b77416fa2"),
            ContractStartDate = DateTime.UtcNow.AddDays(-3),
            Name = "qwerty name",
            StatusCode = "in progress"
        });

        Assert.NotNull(item);

        var get = await this._crudContractService.GetAsync(item.Id);
        Assert.NotNull(get);
    }
}
