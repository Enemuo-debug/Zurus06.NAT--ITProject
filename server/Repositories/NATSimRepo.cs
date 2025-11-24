using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using server.data;
using server.Interfaces;
using server.NATModels;
using server.tools;

// All the functionalies here are for the owner of the Simulation alone
namespace server.Repositories
{
    public class NATSimRepo : ISimulation
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<NATUser> userManager;

        public NATSimRepo(ApplicationDbContext _context, UserManager<NATUser> _userManager)
        {
            context = _context;
            userManager = _userManager;
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

        public async Task<string> VerifyEmbed(int postId, string embedCode, string loggedInId)
        {
            var codes = embedCode.Split("/");
            string ownerId = codes[^2];
            string simulationId = codes[^1];
            var simulation = await GetSimulationById(int.Parse(simulationId));
            var user = await userManager.FindByIdAsync(loggedInId);
            var post = await context.Posts.FindAsync(postId);

            // Ensure the post and all others are owned by the user requesting an embed
            if (user != null && post != null && user.Id == ownerId && post.userId == user.Id && simulation != null && simulation.OwnerId == user.Id)
            {
                return simulation!.Name;
            }
            return "";
        }

        public async Task<string> GetSimulationDataFromEmbed(string embedURL)
        {
            var codes = embedURL.Split("/");
            string simulationId = codes[^1];
            var simulation = await GetSimulationById(int.Parse(simulationId));
            return simulation!.DataJson + "^" +simulation!.Name;
        }

        public async Task<List<string>> SingleSourceShortestPathAlgorithm(Graph simGraph, string startId)
        {
            List<string> output = new();
            string message = simGraph.DijkstraAlgorithm(startId, out output);

            output = output.Prepend(message).ToList();

            return output;
        }

        public async Task<Graph> ConvertSimulationToGraphForAnalysis(int simulationId)
        {
            // Get the simulation first of all
            var simulation = await context.Diagrams.FindAsync(simulationId);
            if (simulation == null)
            {
                return null!;
            }
            string diagram = simulation!.DataJson;
            var obj = JsonSerializer.Deserialize<GraphData>(diagram, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

            if (obj == null) return null;
            Graph outputGraph = new Graph();
            List<string> deviceIds = obj.Devices.Select(d => $"{d.Type}_{d.Id}").ToList();

            // May not be needed but just in case
            if (deviceIds.Count != deviceIds.Distinct().Count())
                return null;
            
            // Add nodes
            bool overallSuccess = outputGraph.AddNodes(deviceIds);
            if (!overallSuccess) return null;

            // Now to the edges
            foreach (var item in obj.Links)
            {
                string fromIndex = $"{item.From.Type}_{item.From.Id}";
                string toIndex = $"{item.To.Type}_{item.To.Id}";
                bool success = outputGraph.AddLink(fromIndex, toIndex, 1) && outputGraph.AddLink(toIndex, fromIndex, 1);
                if (!success)
                {
                    return null;
                }
            }
            return outputGraph;
        }
    }
}