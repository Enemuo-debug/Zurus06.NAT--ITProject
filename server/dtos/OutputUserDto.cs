using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.tools;

namespace server.dtos
{
    public class OutputUserDto
    {
        public required string DisplayName { get; set; }
        public required string niche { get; set; }
        public required string Bio { get; set; }
        public required string JoinedAt { get; set; }
    }
}