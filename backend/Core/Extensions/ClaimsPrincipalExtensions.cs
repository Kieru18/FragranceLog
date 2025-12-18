using System.Security.Claims;

namespace Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(sub, out var parsed))
                return parsed;

            return null;
        }
    }
}
