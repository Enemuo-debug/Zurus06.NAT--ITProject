using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.dtos;
using server.NATModels;

namespace server.MappersAndExtensions
{
    public static class UserExtension
    {
        public static OutputUserDto UserDetails(this NATUser user)
        {
            return new OutputUserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                UserName = user.UserName,
                niche = user.niche.ToString().Replace("_", " "),
                Bio = user.Bio,
                JoinedAt = user.CreatedOn
            };
        }
    }
}