using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.NATModels
{
    public class NATPosts
    {
        public int Id { get; set; }
        public required string userId { get; set; }
        public string Caption { get; set; } = string.Empty;
        public string Intro { get; set; } = string.Empty;
        public int Content { get; set; }
    }
}