namespace KV.Server.Helpers;
using System;
using System.Web;

public static class WebUrlHelper
{
    /// <summary>
    ///     Вид записи "new Uri(https://localhost:7010/Page/Details/1?search="), "search", "123"
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Uri CreateOrUpdateParameterValueUri(this Uri uri, string parameterName, string value)
    {
        var uriBuilder = new UriBuilder(uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query[parameterName] = value;
        uriBuilder.Query = query.ToString();
        var resultUri = uriBuilder.ToString();
        return new Uri(resultUri);
    }

    public static Uri CreateHandlerUri(this Uri uri, string methodName)
    {
        var onPost = "OnPost";
        var onGet = "OnGet";
        var async = "Async";

        var parameterName = methodName;

        if(methodName.Contains(onGet, StringComparison.InvariantCultureIgnoreCase))
        {
            parameterName = parameterName.Replace(onGet, string.Empty);
        }

        if(methodName.Contains(onPost, StringComparison.InvariantCultureIgnoreCase))
        {
            parameterName = parameterName.Replace(onPost, string.Empty);
        }

        if (methodName.Contains(async, StringComparison.InvariantCultureIgnoreCase))
        {
            parameterName = parameterName.Replace(async, string.Empty);
        }

        var uriBuilder = new UriBuilder(uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["handler"] = parameterName;
        uriBuilder.Query = query.ToString();
        var resultUri = uriBuilder.ToString();
        return new Uri(resultUri);
    }
}
