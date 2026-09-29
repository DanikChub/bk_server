namespace KV.Server.BlobStorage;
using System.Globalization;
using System.Text.RegularExpressions;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;

public partial class S3BlobNamingNormalizer : IBlobNamingNormalizer, ITransientDependency
{
    /// <summary>
    ///     https://docs.aws.amazon.com/AmazonS3/latest/dev/BucketRestrictions.html
    /// </summary>
    public virtual string NormalizeContainerName(string containerName)
    {
        using (CultureHelper.Use(CultureInfo.InvariantCulture))
        {
            // All letters in a container name must be lowercase.
            containerName = containerName.ToLowerInvariant();

            // Container names must be from 3 through 63 characters long.
            if (containerName.Length > 63)
            {
                containerName = containerName[..63];
            }

            // Bucket names can consist only of lowercase letters, numbers, dots (.), and hyphens (-).
            containerName = MyRegex().Replace(containerName, string.Empty);

            // Bucket names must begin and end with a letter or number.
            // Bucket names must not be formatted as an IP address (for example, 192.168.5.4).
            // Bucket names can't start or end with hyphens adjacent to period
            // Bucket names can't start or end with dots adjacent to period
            containerName = MyRegex1().Replace(containerName, ".");
            containerName = MyRegex2().Replace(containerName, string.Empty);
            containerName = MyRegex3().Replace(containerName, string.Empty);
            containerName = MyRegex4().Replace(containerName, string.Empty);
            containerName = MyRegex5().Replace(containerName, string.Empty);
            containerName = MyRegex6().Replace(containerName, string.Empty);
            containerName = MyRegex7().Replace(containerName, string.Empty);
            containerName = MyRegex8().Replace(containerName, string.Empty);

            if (containerName.Length < 3)
            {
                var length = containerName.Length;
                for (var i = 0; i < 3 - length; i++)
                {
                    containerName += "0";
                }
            }

            return containerName;
        }
    }

    /// <summary>
    ///     https://docs.aws.amazon.com/AmazonS3/latest/dev/UsingMetadata.html
    /// </summary>
    public virtual string NormalizeBlobName(string blobName) => blobName;
    [GeneratedRegex("[^a-z0-9-.]")]
    private static partial Regex MyRegex();
    [GeneratedRegex("\\.{2,}")]
    private static partial Regex MyRegex1();
    [GeneratedRegex("-\\.")]
    private static partial Regex MyRegex2();
    [GeneratedRegex("\\.-")]
    private static partial Regex MyRegex3();
    [GeneratedRegex("^-")]
    private static partial Regex MyRegex4();
    [GeneratedRegex("-$")]
    private static partial Regex MyRegex5();
    [GeneratedRegex("^\\.")]
    private static partial Regex MyRegex6();
    [GeneratedRegex("\\.$")]
    private static partial Regex MyRegex7();
    [GeneratedRegex("^(?:(?:^|\\.)(?:2(?:5[0-5]|[0-4]\\d)|1?\\d?\\d)){4}$")]
    private static partial Regex MyRegex8();
}
