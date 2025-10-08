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
        public async Task<NATSimulation?> GetSimulationById(int simulationId, string userName)
        {
            return await context.Diagrams.FirstOrDefaultAsync(d => d.OwnerName == userName && d.Id == simulationId);
        }

        public async Task<NATSimulation?> EmbedSimulation(string simUUID)
        {
            var codes = simUUID.Split("_");
            return await context.Diagrams.FirstOrDefaultAsync(s => s.OwnerName == codes[0] && s.Id == int.Parse(codes[1]));
        }

        public async Task<List<NATSimulation>> GetAllUserSimulations(string userName)
        {
            var dGs = context.Diagrams.Where(u => u.OwnerName == userName);
            var list = await dGs.ToListAsync();
            foreach (var item in list)
            {
                item.DecryptNetwork();
            }
            return list;
        }

        // Creates an empty simulation which will now be edited
        public async Task<bool> CreateSimulation (string ownerName)
        {
            var newSim = new NATSimulation { OwnerName = ownerName };
            await context.Diagrams.AddAsync(newSim);
            int result = await context.SaveChangesAsync();
            return result > 0;
        }
        // Now to the editing of the Simulation
        public async Task<bool> UpdateSimulation (int simId, string userName, string devices, string links)
        {
            NATSimulation? toEdit = await GetSimulationById(simId, userName);
            // Check for existence of the Simulation
            if (toEdit == null)
            {
                return false;
            }
            toEdit.devices = devices;
            toEdit.links = links;
            toEdit.EncryptNetwork();
            context.Diagrams.Update(toEdit);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSimulation (int simId, string userName)
        {
            NATSimulation? toDelete = await GetSimulationById(simId, userName);
            // Check for existence of the Simulation
            if (toDelete == null)
            {
                return false;
            }
            context.Diagrams.Remove(toDelete);
            int result = await context.SaveChangesAsync();
            return result > 0;
        }
        
        public string? DijkstraAlgorithm(NATSimulation simulation, string start)
        {
            var mappings = new HashSet<string>();
            Graph simGraph = simulation.ConvertToGraph(out mappings);
            var mapList = mappings.ToList();
            int index = mapList.IndexOf(start);
            if (index == -1) return null;
            string pathDesc = simGraph.DijkstraAlgorithm(index, mapList);
            return pathDesc;
        }
    }
}