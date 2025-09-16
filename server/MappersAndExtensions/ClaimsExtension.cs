using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace server.MappersAndExtensions
{
    public static class ClaimsExtension
    {
        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst("Email-Address").Value;
        }
    }
}