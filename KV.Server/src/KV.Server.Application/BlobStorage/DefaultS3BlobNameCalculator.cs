namespace KV.Server.BlobStorage;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

public class DefaultS3BlobNameCalculator : IS3BlobNameCalculator, ITransientDependency
{
    public DefaultS3BlobNameCalculator(ICurrentTenant currentTenant)
    {
        this.CurrentTenant = currentTenant;
    }

    protected ICurrentTenant CurrentTenant { get; }

    public virtual string Calculate(BlobProviderArgs args) => this.CurrentTenant.Id == null
            ? $"host/{args.BlobName}"
            : $"tenants/{this.CurrentTenant.Id.Value:D}/{args.BlobName}";
}
