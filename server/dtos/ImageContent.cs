using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.tools;

namespace server.dtos
{
    public class ImageContent : OutputContentGroup
    {
        public required int Id { get; set; }
        public required string ImgLink { get; set; }
        public required string Content { get; set; }
    }
}