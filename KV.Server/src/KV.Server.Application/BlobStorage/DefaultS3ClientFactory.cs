namespace KV.Server.BlobStorage;
using System;
using System.Threading.Tasks;
using Amazon.S3;
using Volo.Abp.DependencyInjection;

public class DefaultS3ClientFactory : IS3ClientFactory, ITransientDependency
{
    public virtual Task<AmazonS3Client> GetS3Client(
        S3BlobProviderConfiguration configuration) => Task.FromResult(new AmazonS3Client(configuration.AccessKeyId, configuration.SecretAccessKey,
            this.GetS3Configuration(configuration)));

    protected virtual AmazonS3Config GetS3Configuration(
        S3BlobProviderConfiguration configuration)
    {
        if (configuration.ServiceURL.IsNullOrWhiteSpace())
        {
            return null;
        }

        var config = new AmazonS3Config
        {
            ServiceURL = configuration.ServiceURL
        };

        return config;
    }
}
