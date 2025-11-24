using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.NATModels;
using server.tools;

namespace server.Interfaces
{
    public interface ISimulation
    {
        Task<bool> CreateNewSimulation(string name, string ownerId, string initialDataJson = "{}");
        Task<NATSimulation?> GetSimulationById(int simulationId);
        Task<List<NATSimulation>> GetSimulationsByOwnerId(string ownerId);
        Task<bool> UpdateSimulationData(int simulationId, string dataJson);
        Task<bool> DeleteSimulation(int simulationId);
        Task<string> VerifyEmbed (int postId, string embedCode, string loggedInId);
        Task<string> GetSimulationDataFromEmbed(string embedURL);
        Task<List<string>> SingleSourceShortestPathAlgorithm (Graph simGraph, string startId);
        Task<Graph> ConvertSimulationToGraphForAnalysis (int simulationId);
    }
}