namespace KV.Server.BlobStorage;
using Appricot.S3BlobStorage;
using Volo.Abp.BlobStoring;

public class S3BlobProviderConfiguration
{
    private readonly BlobContainerConfiguration _containerConfiguration;

    public S3BlobProviderConfiguration(BlobContainerConfiguration containerConfiguration)
    {
        this._containerConfiguration = containerConfiguration;
    }

    public string AccessKeyId {
        get => this._containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.AccessKeyId);
        set => this._containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.AccessKeyId, value);
    }

    public string SecretAccessKey {
        get => this._containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames
            .SecretAccessKey);
        set => this._containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.SecretAccessKey, value);
    }

    public string ServiceURL {
        get => this._containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.ServiceURL);
        set => this._containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.ServiceURL, value);
    }

    /// <summary>
    ///     Default value: false.
    /// </summary>
    public bool CreateContainerIfNotExists {
        get => this._containerConfiguration.GetConfigurationOrDefault(
            S3BlobProviderConfigurationNames.CreateContainerIfNotExists, false);
        set => this._containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.CreateContainerIfNotExists,
            value);
    }

    /// <summary>
    ///     This name may only contain lowercase letters, numbers, and hyphens, and must begin with a letter or a number.
    ///     Each hyphen must be preceded and followed by a non-hyphen character.
    ///     The name must also be between 3 and 63 characters long.
    ///     If this parameter is not specified, the ContainerName of the <see cref="BlobProviderArgs" /> will be used.
    /// </summary>
    public string ContainerName {
        get =>
            this._containerConfiguration.GetConfigurationOrDefault<string>(S3BlobProviderConfigurationNames.ContainerName);
        set => this._containerConfiguration.SetConfiguration(S3BlobProviderConfigurationNames.ContainerName, value);
    }
}
