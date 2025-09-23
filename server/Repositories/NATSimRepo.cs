using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.data;
using server.Interfaces;
using server.NATModels;

namespace server.Repositories
{
    public class NATSimRepo : ISimulation
    {
        private readonly ApplicationDbContext context;
        public NATSimRepo(ApplicationDbContext _context)
        {
            context = _context;
        }
        public async Task<NATSimulation?> GetSimulationById(int simulationId)
        {
            return await context.Diagrams.FindAsync(simulationId);
        }
    }
}