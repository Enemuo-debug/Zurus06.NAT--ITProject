using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using server.data;
using server.Interfaces;
using server.MappersAndExtensions;
using server.NATModels;
using server.tools;

// All the functionalies here are for the owner of the Simulation alone
namespace server.Repositories
{
    public class NATSimRepo : ISimulation
    {
        private readonly ApplicationDbContext context;

        public NATSimRepo(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task<bool> CreateNewSimulation(string name, string ownerId, string initialDataJson = "{}")
        {
            await context.Diagrams.AddAsync(new NATSimulation
            {
                Name = name,
                OwnerId = ownerId,
                DataJson = initialDataJson
            });

            int result = await context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteSimulation(int simulationId)
        {
            NATSimulation? simulation = await GetSimulationById(simulationId);
            if (simulation == null) return false;

            context.Diagrams.Remove(simulation);
            int result = await context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<NATSimulation?> GetSimulationById(int simulationId)
        {
            return await context.Diagrams.FindAsync(simulationId);
        }

        public async Task<List<NATSimulation>> GetSimulationsByOwnerId(string ownerId)
        {
            var simulations = context.Diagrams
                .Where(sim => sim.OwnerId == ownerId);
            return await simulations.ToListAsync();
        }

        public async Task<bool> UpdateSimulationData(int simulationId, string dataJson)
        {
            var simulation = await GetSimulationById(simulationId);
            if (simulation == null) return false;
            simulation.DataJson = dataJson;
            context.Diagrams.Update(simulation);
            int result = await context.SaveChangesAsync();
            return result > 0;
        }
    }
}