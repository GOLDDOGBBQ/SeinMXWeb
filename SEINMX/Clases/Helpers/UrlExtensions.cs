namespace SEINMX.Clases.Helpers;

using Microsoft.AspNetCore.Http;

public static class UrlExtensions
{
    private static IHttpContextAccessor? _accessor;

    public static void Configure(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public static string Absolute(string relativePath)
    {
        if (_accessor?.HttpContext == null)
            return relativePath; // fallback

        var req = _accessor.HttpContext.Request;

        if (!relativePath.StartsWith("/"))
            relativePath = "/" + relativePath;

        return $"{req.Scheme}://{req.Host}{relativePath}";
    }
}