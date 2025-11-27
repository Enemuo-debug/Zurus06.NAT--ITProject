using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.dtos
{
    public class CommentOutputDto
    {
        public required int Id { get; set; }
        public required string UserId { get; set; }
        public required string DisplayName { get; set; }
        public required string Niche { get; set; }
        public required string Message { get; set; }
    }
}
