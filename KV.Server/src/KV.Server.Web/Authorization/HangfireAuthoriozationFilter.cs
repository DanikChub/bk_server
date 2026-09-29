using Hangfire.Dashboard;

namespace KV.Server.Web.Authorization;

public class HangfireAuthoriozationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return context.GetHttpContext().User.Identity.IsAuthenticated;
    }
}
