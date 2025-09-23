using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.dtos
{
    public class UserDetailsDto
    {
        public required OutputUserDto UserDetails { get; set; }
        public required List<OutputPostDto> UserPosts { get; set; }
    }
}