namespace KV.Server.BlobStorage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

public class S3CompatibleBlobProvider : BlobProviderBase, ITransientDependency
{
    public S3CompatibleBlobProvider(
        IS3BlobNameCalculator blobNameCalculator,
        IS3ClientFactory s3ClientFactory,
        IBlobNormalizeNamingService blobNormalizeNamingService)
    {
        this.BlobNameCalculator = blobNameCalculator;
        this.S3ClientFactory = s3ClientFactory;
        this.BlobNormalizeNamingService = blobNormalizeNamingService;
    }

    protected IS3BlobNameCalculator BlobNameCalculator { get; }
    protected IS3ClientFactory S3ClientFactory { get; }
    protected IBlobNormalizeNamingService BlobNormalizeNamingService { get; }

    public override async Task<bool> DeleteAsync(BlobProviderDeleteArgs args)
    {
        var blobName = this.BlobNameCalculator.Calculate(args);
        var containerName = this.GetContainerName(args);

        using (var amazonS3Client = await this.GetS3Client(args))
        {
            if (!await this.BlobExistsAsync(amazonS3Client, containerName, blobName))
            {
                return false;
            }

            await amazonS3Client.DeleteObjectAsync(containerName, blobName);

            return true;
        }
    }

    public override async Task<bool> ExistsAsync(BlobProviderExistsArgs args)
    {
        var blobName = this.BlobNameCalculator.Calculate(args);
        var containerName = this.GetContainerName(args);

        using (var amazonS3Client = await this.GetS3Client(args))
        {
            return await this.BlobExistsAsync(amazonS3Client, containerName, blobName);
        }
    }

    public override async Task<Stream> GetOrNullAsync(BlobProviderGetArgs args)
    {
        var blobName = this.BlobNameCalculator.Calculate(args);
        var containerName = this.GetContainerName(args);

        using (var amazonS3Client = await this.GetS3Client(args))
        {
            if (!await this.BlobExistsAsync(amazonS3Client, containerName, blobName))
            {
                return null;
            }

            var response = await amazonS3Client.GetObjectAsync(containerName, blobName);

            return await this.TryCopyToMemoryStreamAsync(response.ResponseStream, args.CancellationToken);
        }
    }

    public override async Task SaveAsync(BlobProviderSaveArgs args)
    {
        var blobName = this.BlobNameCalculator.Calculate(args);
        var configuration = args.Configuration.GetS3Configuration();
        var containerName = this.GetContainerName(args);

        using (var amazonS3Client = await this.GetS3Client(args))
        {
            if (!args.OverrideExisting && await this.BlobExistsAsync(amazonS3Client, containerName, blobName))
            {
                throw new BlobAlreadyExistsException(
                    $"Saving BLOB '{args.BlobName}' does already exists in the container '{containerName}'! Set {nameof(args.OverrideExisting)} if it should be overwritten.");
            }

            if (configuration.CreateContainerIfNotExists)
            {
                await this.CreateContainerIfNotExists(amazonS3Client, containerName);
            }

            await amazonS3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = containerName,
                Key = blobName,
                InputStream = args.BlobStream
            });
        }
    }

    protected virtual async Task<AmazonS3Client> GetS3Client(BlobProviderArgs args)
    {
        var configuration = args.Configuration.GetS3Configuration();
        return await this.S3ClientFactory.GetS3Client(configuration);
    }

    protected virtual async Task<bool> BlobExistsAsync(AmazonS3Client amazonS3Client, string containerName,
        string blobName)
    {
        // Make sure Blob Container exists.
        try
        {
            if (!await AmazonS3Util.DoesS3BucketExistV2Async(amazonS3Client, containerName))
            {
                return false;
            }

        }
        catch (AmazonS3Exception)
        {
            return false;
        }
  
        try
        {
            await amazonS3Client.GetObjectMetadataAsync(containerName, blobName);
        }
        catch (Exception ex)
        {
            if (ex is AmazonS3Exception)
            {
                return false;
            }

            throw;
        }

        return true;
    }

    protected virtual async Task CreateContainerIfNotExists(AmazonS3Client amazonS3Client, string containerName)
    {
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(amazonS3Client, containerName))
        {
            await amazonS3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = containerName
            });
        }
    }

    protected virtual string GetContainerName(BlobProviderArgs args)
    {
        var configuration = args.Configuration.GetS3Configuration();
        return configuration.ContainerName.IsNullOrWhiteSpace()
            ? args.ContainerName
            : this.BlobNormalizeNamingService.NormalizeContainerName(args.Configuration, configuration.ContainerName);
    }

    protected override async Task<Stream> TryCopyToMemoryStreamAsync(Stream stream,
        CancellationToken cancellationToken = default)
    {
        if (stream == null)
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
