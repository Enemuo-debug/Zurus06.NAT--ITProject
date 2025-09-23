using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using server.data;
using server.NATModels;
using Microsoft.EntityFrameworkCore;

namespace server.MappersAndExtensions
{
    public static class ClaimsExtension
    {
        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            if (user == null) return null;
            if (!user.Identity.IsAuthenticated) return null;
            return user.FindFirst("Email-Address").Value;
        }

        public async static Task<NATUser?> GetUser(this ClaimsPrincipal user, ApplicationDbContext context)
        {
            if (user == null) return null;
            if (!user.Identity.IsAuthenticated) return null;
            return await context.Users.FirstOrDefaultAsync(c => c.Email == user.FindFirst("Email-Address").Value);
        }
    }
}