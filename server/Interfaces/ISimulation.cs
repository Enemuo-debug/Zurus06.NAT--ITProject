using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.NATModels;

namespace server.Interfaces
{
    public interface ISimulation
    {
        Task<bool> CreateNewSimulation(string name, string ownerId, string initialDataJson = "{}");
        Task<NATSimulation?> GetSimulationById(int simulationId);
        Task<List<NATSimulation>> GetSimulationsByOwnerId(string ownerId);
        Task<bool> UpdateSimulationData(int simulationId, string dataJson);
        Task<bool> DeleteSimulation(int simulationId);
    }
}