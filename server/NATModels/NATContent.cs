using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.tools;

namespace server.NATModels
{
    public class NATContent
    {
        public int Id { get; set; }
        public ContentTypes type = ContentTypes.Text;
        public string ImgLink { get; set; } = "#";
        public string Content { get; set; } = string.Empty;
        public int Link { get; set; }
    }
}