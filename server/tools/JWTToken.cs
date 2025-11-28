using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using server.NATModels;

namespace server.tools
{
    public class JWTToken
    {
        public string CreateToken (NATUser user)
        {
            var claims = new[]
            {
                new Claim("Email-Address", user.Email ?? string.Empty),
                new Claim("Niche", user.niche.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("key") ??string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: Environment.GetEnvironmentVariable("Issuer"),
                audience: Environment.GetEnvironmentVariable("Audience"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(240),
                signingCredentials: creds
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}