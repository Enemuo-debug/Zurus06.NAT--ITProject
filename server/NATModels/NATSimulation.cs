using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.NATModels
{
    public class NATSimulation
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        // Id of the user who owns this simulation
        public required string OwnerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Store your entire simulation as JSON
        public required string DataJson { get; set; }
    }

}