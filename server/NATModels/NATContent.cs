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
        public required ContentTypes type { get; set; } = ContentTypes.Text;
        public string ImgLink { get; set; } = "#";
        public string Content { get; set; } = string.Empty;
        public int NATSimulationId { get; set; }
        public int Link { get; set; }
        public required string Owner { get; set; }
    }
}