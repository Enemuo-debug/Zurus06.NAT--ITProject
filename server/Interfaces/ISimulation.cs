using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.NATModels;

namespace server.Interfaces
{
    public interface ISimulation
    {
        Task<NATSimulation?> GetSimulationById(int simulationId);
    }
}