using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.NATModels;
using server.tools;

namespace server.dtos
{
    public class OutputPostDto
    {
        public required int Id { get; set; }
        public required string creatorId { get; set; }
        public string Caption { get; set; } = string.Empty;
        public string Intro { get; set; } = string.Empty;
        public List<OutputContentGroup> Contents { get; set; } = new List<OutputContentGroup>();
    }
}