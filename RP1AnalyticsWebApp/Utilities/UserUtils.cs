using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace RP1AnalyticsWebApp.Utilities
{
    public static class UserUtils
    {
        public static string[] GetRoles(this ClaimsPrincipal user)
        {
            IEnumerable<Claim> roleClaims = user.FindAll(ClaimTypes.Role);
            return roleClaims.Select(r => r.Value).ToArray();
        }
    }
}
