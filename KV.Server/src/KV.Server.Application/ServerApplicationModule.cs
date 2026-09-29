using Volo.Abp.Security;
using Volo.Abp.Caching;
using System.IO;
using KV.Server.BlobStorage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace KV.Server;

[DependsOn(
    typeof(ServerDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(ServerApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpEventBusRabbitMqModule),
    typeof(AbpBlobStoringModule),
    typeof(AbpBlobStoringFileSystemModule),
    typeof(AbpSecurityModule),
    typeof(AbpCachingModule))]

    public class ServerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ICrudContractService, CrudContractService>();
        context.Services.AddTransient<ICrudContractStatusService, CrudContractStatusService>();

        this.Configure<AbpAutoMapperOptions>(options => options.AddMaps<ServerApplicationModule>());
        this.Configure<AbpDistributedEntityEventOptions>(options =>
        {
            options.AutoEventSelectors.Add<Tenant>();
            options.EtoMappings.Add<Tenant, TenantEto>();
        });

        this.Configure<AbpRabbitMqEventBusOptions>(options => options.ClientName = ServerApplicationConsts.ClientName);
        var configuration = context.Services.GetConfiguration();
        var environment = context.Services.GetAbpHostEnvironment();
        if (environment.IsDevelopment())
        {
            this.Configure<AbpBlobStoringOptions>(options =>
            {
                options.Containers.Configure(BlobContainers.DEFAULT_PUBLIC, container => container.UseFileSystem(fileSystem =>
                {
                    fileSystem.BasePath = Path.Combine(
                        configuration["UploadedFilePath"] ?? StorageDirectories.GetLocalStoragePath(),
                        StorageSettings.DEBUGLOCALSTORAGEDIRECTORY);
                }));

                options.Containers.Configure(BlobContainers.ATTACHMENTS, container => container.UseFileSystem(fileSystem => fileSystem.BasePath = Path.Combine(
                           configuration["UploadedFilePath"] ?? StorageDirectories.GetLocalStoragePath(),
                            StorageSettings.DEBUGLOCALTICKETSSTORAGEDIRECTORY)));

                options.Containers.Configure(BlobContainers.STORIES, container => container.UseFileSystem(fileSystem => fileSystem.BasePath = Path.Combine(
                        configuration["UploadedFilePath"] ?? StorageDirectories.GetLocalStoragePath(),
                            StorageSettings.DEBUGLOCALSTORIESSTORAGEDIRECTORY)));

                options.Containers.Configure(BlobContainers.AVATAR_EMPLOYEE, container => container.UseFileSystem(fileSystem => fileSystem.BasePath = Path.Combine(
                        configuration["UploadedFilePath"] ?? StorageDirectories.GetLocalStoragePath(),
                            StorageSettings.DEBUGLOCALAVATARDIRECTORY)));

                options.Containers.ConfigureDefault(container => container.UseFileSystem(fileSystem => fileSystem.BasePath = Path.Combine(
                         configuration["UploadedFilePath"] ?? StorageDirectories.GetLocalStoragePath(),
                            StorageSettings.DEBUGLOCALSTORAGEDIRECTORY)));
            });
        }
        else
        {
            bool.TryParse(configuration["S3:CreateContainerIfNotExists"] ?? "true",
                out bool createContainerIfNotExists);
            var accessKeyId = configuration["S3:AccessKeyId"];
            var secretAccessKey = configuration["S3:SecretAccessKey"];
            var serviceURL = configuration["S3:BaseUrl"] ?? "https://s3.yandexcloud.net";

            Configure<AbpBlobStoringOptions>(options =>
            {
                options.Containers.Configure(BlobContainers.DEFAULT_PUBLIC, container =>
                {
                    container.UseS3Storage(config =>
                    {
                        config.ContainerName =
     configuration["S3:Containers:" + BlobContainers.DEFAULT_PUBLIC + ":Name"] ?? BlobContainers.DEFAULT_PUBLIC;
                        config.CreateContainerIfNotExists = createContainerIfNotExists;
                        config.AccessKeyId = accessKeyId;
                        config.SecretAccessKey = secretAccessKey;
                        config.ServiceURL = serviceURL;
                    });
                });

                options.Containers.Configure(BlobContainers.ATTACHMENTS, container =>
                {
                    container.UseS3Storage(config =>
                    {
                        config.ContainerName =
     configuration["S3:Containers:" + BlobContainers.ATTACHMENTS + ":Name"] ?? BlobContainers.ATTACHMENTS;
                        config.CreateContainerIfNotExists = createContainerIfNotExists;
                        config.AccessKeyId = accessKeyId;
                        config.SecretAccessKey = secretAccessKey;
                        config.ServiceURL = serviceURL;
                    });
                });

                options.Containers.Configure(BlobContainers.STORIES, container =>
                {
                    container.UseS3Storage(config =>
                    {
                        config.ContainerName =
     configuration["S3:Containers:" + BlobContainers.STORIES + ":Name"] ?? BlobContainers.STORIES;
                        config.CreateContainerIfNotExists = createContainerIfNotExists;
                        config.AccessKeyId = accessKeyId;
                        config.SecretAccessKey = secretAccessKey;
                        config.ServiceURL = serviceURL;
                    });
                });

                options.Containers.Configure(BlobContainers.AVATAR_EMPLOYEE, container =>
                {
                    container.UseS3Storage(config =>
                    {
                        config.ContainerName =
     configuration["S3:Containers:" + BlobContainers.AVATAR_EMPLOYEE + ":Name"] ?? BlobContainers.AVATAR_EMPLOYEE;
                        config.CreateContainerIfNotExists = createContainerIfNotExists;
                        config.AccessKeyId = accessKeyId;
                        config.SecretAccessKey = secretAccessKey;
                        config.ServiceURL = serviceURL;
                    });
                });

                options.Containers.ConfigureDefault(container =>
                {
                    container.UseS3Storage(config =>
                    {
                        config.ContainerName =
     configuration["S3:Containers:" + BlobContainers.DEFAULT_PUBLIC + ":Name"] ?? BlobContainers.DEFAULT_PUBLIC;
                        config.CreateContainerIfNotExists = createContainerIfNotExists;
                        config.AccessKeyId = accessKeyId;
                        config.SecretAccessKey = secretAccessKey;
                        config.ServiceURL = serviceURL;
                    });
                });
            });
        }
    }
}
