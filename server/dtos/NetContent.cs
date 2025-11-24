using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.NATModels;
using server.tools;

namespace server.dtos
{
    public class NetContent : OutputContentGroup
    {
        public required int Id { get; set; }
        public required string NATSimulation { get; set; }
    }
}