namespace SEINMX.Clases.Helpers;

using System.Security.Claims;

public static  class UserClaimsHelper
{
    public static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string? GetApiName(string controller, string action)
    {
        return $"{controller}.{action}".ToUpper();
    }

    public static bool IsAdmin(ClaimsPrincipal user)
    {
        var sAdmin = user.FindFirstValue("Admin");

        if (!bool.TryParse(sAdmin, out bool isAdmin))
            return false;

        return isAdmin;
    }

    public static string? GetClaim(ClaimsPrincipal user, string claimName)
    {
        return user.FindFirstValue(claimName);
    }
}