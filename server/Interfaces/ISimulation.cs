using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.NATModels;

namespace server.Interfaces
{
    public interface ISimulation
    {
        Task<NATSimulation?> GetSimulationById(int simulationId, string userName);
        Task<NATSimulation?> EmbedSimulation(string simUUID);
        Task<List<NATSimulation>> GetAllUserSimulations(string userName);
        Task<bool> CreateSimulation(string ownerName);
        Task<bool> UpdateSimulation(int simId, string userName, string devices, string links);
        Task<bool> DeleteSimulation(int simId, string userName);

        // Graph Operations
        string? DijkstraAlgorithm(NATSimulation simulation, string start);
    }
}