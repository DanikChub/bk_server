namespace KV.Server.BlobStorage;
using System.Threading.Tasks;
using Amazon.S3;

public interface IS3ClientFactory
{
    Task<AmazonS3Client> GetS3Client(S3BlobProviderConfiguration configuration);
}
