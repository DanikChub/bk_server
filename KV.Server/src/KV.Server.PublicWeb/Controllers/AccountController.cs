namespace KV.Server.PublicWeb.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Users;

[Route("custom-account")]
public class AccountController : AbpController
{
    public const string ExternalApiKeyName = "ExternalApiKey";
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;

    public AccountController(IConfiguration configuration,
        ICurrentUser currentUser,
        string[]? challengeAuthenticationSchemas = null)
    {
        this._configuration = configuration;
        this._currentUser = currentUser;
        this.ChallengeAuthenticationSchemas = challengeAuthenticationSchemas ?? new[] { "oidc" };
        this.AuthenticationType = "Identity.Application";
        this.ForbidSchemes = Array.Empty<string>();
    }

    protected string[] ChallengeAuthenticationSchemas { get; }
    protected string AuthenticationType { get; }
    protected string[] ForbidSchemes { get; }

    [HttpGet("SignOff")]
    public virtual async Task<ActionResult> LogoutUserAsync(
        string returnUrl = "", string returnUrlHash = "")
    {
        await this.HttpContext.SignOutAsync();

        if (this.HttpContext.User.Identity?.AuthenticationType == this.AuthenticationType)
        {
            return this.RedirectSafely(returnUrl, returnUrlHash);
        }

        return this.SignOut(new AuthenticationProperties
        {
            RedirectUri = this.GetRedirectUrl(returnUrl, returnUrlHash)
        },
            this.ChallengeAuthenticationSchemas);
    }

    [Authorize]
    [HttpGet("AuthorizeResource")]
    public virtual ActionResult AuthorizeResource(string resId, string source)
    {
        var accessToken = this.GetAccessToken();

        var externalApiKey = this._configuration.GetValue<string>(ExternalApiKeyName);
        if (resId != externalApiKey)
        {
            throw new UserFriendlyException("Вы не авторизованы");
        }

        var uriBuilder = new UriBuilder(source);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["token"] = accessToken;
        uriBuilder.Query = query.ToString();
        var url = uriBuilder.ToString();
        return this.RedirectSafely(url);
    }

    private string GetAccessToken()
    {
        var key = Encoding.ASCII.GetBytes("as3fdv3sfras3fdv3sfras3fdv3sfras3fdv3sfras3");
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var token = jwtTokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("UserName", this._currentUser?.UserName ?? "")
            }),
            Expires = DateTime.UtcNow.AddHours(6),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
        });
        var accessToken = jwtTokenHandler.WriteToken(token);
        return accessToken;
    }
}
