namespace KV.Server.PublicWeb;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Abp.AspNetCore.Mvc.UI.Theme.KvPublic;
using Abp.AspNetCore.Mvc.UI.Theme.KvPublic.Bundling;
using KV.Server.EntityFrameworkCore;
using KV.Server.Localization;
using KV.Server.MultiTenancy;
using KV.Server.Permissions;
using KV.Server.PublicWeb.Menus;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.BlobStoring;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Identity.Web;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.SettingManagement.Web;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

[DependsOn(
    typeof(ServerHttpApiModule),
    typeof(ServerApplicationModule),
    typeof(ServerEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpSettingManagementWebModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreMvcUiKvPublicThemeModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpBlobStoringModule),
    typeof(AbpEventBusRabbitMqModule))]
public class ServerPublicWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        this.PreConfigure<OpenIddictBuilder>(builder => builder.AddValidation(options =>
            {
                options.AddAudiences("Server");
                options.UseLocalServer();
                options.UseAspNetCore();
            }));

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options => options.AddAssemblyResource(
                typeof(ServerResource),
                typeof(ServerDomainModule).Assembly,
                typeof(ServerDomainSharedModule).Assembly,
                typeof(ServerApplicationModule).Assembly,
                typeof(ServerApplicationContractsModule).Assembly,
                typeof(ServerPublicWebModule).Assembly
            ));

        if (!hostingEnvironment.IsDevelopment())
        {
            this.PreConfigure<AbpOpenIddictAspNetCoreOptions>(options => options.AddDevelopmentEncryptionAndSigningCertificate = false);
            this.PreConfigure<OpenIddictServerBuilder>(builder =>
            {
                builder.UseAspNetCore()
                    .EnableStatusCodePagesIntegration()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableVerificationEndpointPassthrough()
                    .DisableTransportSecurityRequirement();
                builder.AddSigningCertificate(GetSigningCertificate(hostingEnvironment, configuration));
                builder.AddEncryptionCertificate(GetEncryptionCertificate(hostingEnvironment, configuration));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        this.ConfigureUrls(configuration);
        this.ConfigureBundles();
        ConfigureSameSiteCookies(context);
        ConfigureAuthentication(context, configuration);
        this.ConfigureAutoMapper();
        this.ConfigureVirtualFileSystem(hostingEnvironment);
        this.Configure<FormOptions>(x =>
        {
            // Set the limit to 1 GB
            x.ValueLengthLimit = 1073741824;
            x.MultipartBodyLengthLimit = 1073741824;
            x.MultipartHeadersLengthLimit = 1073741824;
        });
        this.ConfigureLocalizationServices();
        this.ConfigureNavigationServices();
        this.ConfigureAutoApiControllers();
        ConfigureCors(context, configuration);
        ConfigureSwaggerServices(context.Services); //ConfigureMultiTenancy();


        //Add permission configs here
        //this.Configure<RazorPagesOptions>(options =>
        //    options.Conventions.AuthorizePage(
        //        "/page-address",
        //        <permission_name>));
    }

    private static X509Certificate2 GetEncryptionCertificate(IWebHostEnvironment hostingEnv, IConfiguration configuration)
    {
        var fileName = configuration["SigningCertificatePath"] ?? Path.Combine("certs", "encryption.pfx");
        var passPhrase = configuration["SigningCertificatePassPhrase"] ?? "780F3C11-0A96-40DE-B335-9848BE88C77D";
        var filePath = Path.Combine(hostingEnv.ContentRootPath, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            using var algorithm = RSA.Create(2048);

            var subject = new X500DistinguishedName("CN=Appricot Encryption Certificate");
            var request =
                new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, true));

            var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(2));
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                var fodlerPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(fodlerPath))
                {
                    Directory.CreateDirectory(fodlerPath);
                }
            }

            System.IO.File.WriteAllBytes(filePath, certificate.Export(X509ContentType.Pfx, passPhrase));
        }

        return new X509Certificate2(filePath, passPhrase);
    }

    private static X509Certificate2 GetSigningCertificate(IWebHostEnvironment hostingEnv, IConfiguration configuration)
    {
        var fileName = configuration["SigningCertificatePath"] ?? Path.Combine("certs", "signing.pfx");
        var passPhrase = configuration["SigningCertificatePassPhrase"] ?? "780F3C11-0A96-40DE-B335-9848BE88C77D";
        var filePath = Path.Combine(hostingEnv.ContentRootPath, fileName);

        if (!string.IsNullOrEmpty(filePath) && !System.IO.File.Exists(filePath))
        {
            using var algorithm = RSA.Create(2048);

            var subject = new X500DistinguishedName("CN=Appricot Signing  Certificate");
            var request =
                new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));

            var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(2));
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                var fodlerPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(fodlerPath))
                {
                    Directory.CreateDirectory(fodlerPath);
                }
            }

            System.IO.File.WriteAllBytes(filePath, certificate.Export(X509ContentType.Pfx, passPhrase));
        }

        return new X509Certificate2(filePath, passPhrase);
    }

    private static void ConfigureSameSiteCookies(
        ServiceConfigurationContext context) =>
            context.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

    private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
    {
        if (options.SameSite == SameSiteMode.None)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            //if (!httpContext.Request.IsHttps || DisallowsSameSiteNone(userAgent))
            if (DisallowsSameSiteNone(userAgent))
            {
                // For .NET Core < 3.1 set SameSite = (SameSiteMode)(-1)
                options.SameSite = SameSiteMode.Unspecified;
            }
        }
    }

    private static bool DisallowsSameSiteNone(string userAgent)
    {
        // Cover all iOS based browsers here. This includes:
        // - Safari on iOS 12 for iPhone, iPod Touch, iPad
        // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
        // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
        // All of which are broken by SameSite=None, because they use the iOS networking stack
        if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
        {
            return true;
        }

        // Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
        // - Safari on Mac OS X.
        // This does not include:
        // - Chrome on Mac OS X
        // Because they do not use the Mac OS networking stack.
        if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
            userAgent.Contains("Version/") && userAgent.Contains("Safari"))
        {
            return true;
        }

        // Cover Chrome 50-69, because some versions are broken by SameSite=None,
        // and none in this range require it.
        // Note: this covers some pre-Chromium Edge versions,
        // but pre-Chromium Edge does not require SameSite=None.
        if (userAgent.Contains("Chrome"))
        {
            return true;
        }

        return false;
    }

    private static void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration) => context.Services.AddCors(options => options.AddDefaultPolicy(builder => builder
        .WithOrigins(
            configuration["App:CorsOrigins"]?
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.RemovePostFix("/"))
                .ToArray() ?? new string[1] { "*" }
        )
        .WithAbpExposedHeaders()
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

    private void ConfigureUrls(IConfiguration configuration) => this.Configure<AppUrlOptions>(options => options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"]);

    private void ConfigureBundles() => this.Configure<AbpBundlingOptions>(options => options.StyleBundles.Configure(
            KvPublicThemeBundles.Styles.Global,
            bundle =>
            {
                bundle.AddFiles("/css/common-fonts.css");
                bundle.AddFiles("/global-styles.css");
            }
        ));

    private static void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        context.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = "Server";
            });
    }

    private void ConfigureAutoMapper() => this.Configure<AbpAutoMapperOptions>(options => options.AddMaps<ServerPublicWebModule>());

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            this.Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<ServerDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}KV.Server.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<ServerDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}KV.Server.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<ServerApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}KV.Server.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<ServerApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}KV.Server.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<ServerPublicWebModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureLocalizationServices() => this.Configure<AbpLocalizationOptions>(options =>
                                                         {
                                                             options.Languages.Add(new LanguageInfo("en", "en", "English"));
                                                             options.Languages.Add(new LanguageInfo("ru", "ru", "Русский"));
                                                         });

    private void ConfigureNavigationServices() => this.Configure<AbpNavigationOptions>(options => options.MenuContributors.Add(new ServerMenuContributor()));

    private void ConfigureAutoApiControllers() => this.Configure<AbpAspNetCoreMvcOptions>(options => options.ConventionalControllers.Create(typeof(ServerApplicationModule).Assembly));

    private static void ConfigureSwaggerServices(IServiceCollection services)
    {
        var config = services.GetConfiguration();
        var authority = config["AuthServer:Authority"] ?? "https://localhost:3000";
        var scopes =
            new
                Dictionary<string, string> /* Requested scopes for authorization code request and descriptions for swagger UI only */
                {
                    { "Server", "Server API" }
                };
        services.AddAbpSwaggerGenWithOAuth(
            authority,
            scopes,
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Server API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseAbpRequestLocalization();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseForwardedHeaders();
        }
        else
        {
            app.UseForwardedHeaders();
            app.UseHsts();
        }

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();
        app.UseJwtTokenMiddleware();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Server API");
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
