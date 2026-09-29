namespace KV.Server.BlobStorage;
using Volo.Abp.BlobStoring;

public interface IS3BlobNameCalculator
{
    string Calculate(BlobProviderArgs args);
}
