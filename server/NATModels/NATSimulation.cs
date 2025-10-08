using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.NATModels
{
    public class NATSimulation
    {
        public int Id { get; set; }
        public string devices { get; set; } = string.Empty;
        public string links { get; set; } = string.Empty;
        public required string OwnerName { get; set; }
    }
}