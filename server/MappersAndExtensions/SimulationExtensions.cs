using server.tools;
using server.NATModels;

namespace server.MappersAndExtensions;

public static class SimulationExtensions
{
    public static Graph ConvertToGraph(this NATSimulation simulation, out HashSet<string> mappings)
    {
        string networkString = Cipher.HillCipherDecrypt(simulation.links);
        string[] netArray = networkString.Split(",");
        Graph outputGraph = new Graph();
        // Populate the mappings set for name look up purposes
        mappings = [];
        for (int i = 0; i < netArray.Length; i++)
        {
            string[] toFro = netArray[i].Split("-");
            mappings.Add(toFro[0]);
            mappings.Add(toFro[1]);
        }
        // Do same for Graph..
        for (int i = 0; i < mappings.Count; i++)
        {
            outputGraph.AddNode(i);
        }
        for (int i = 0; i < netArray.Length; i++)
        {
            string[] toFro = netArray[i].Split("-");
            outputGraph.AddLink(mappings.IndexValue(toFro[0]), mappings.IndexValue(toFro[0]), 1);
        }

        return outputGraph;
    }

    public static void DecryptNetwork(this NATSimulation simulation)
    {
        simulation.devices = Cipher.HillCipherDecrypt(simulation.devices);
        simulation.links = Cipher.HillCipherDecrypt(simulation.links);
    }
    public static void EncryptNetwork(this NATSimulation simulation)
    {
        simulation.devices = Cipher.HillCipherEncrypt(simulation.devices);
        simulation.links = Cipher.HillCipherEncrypt(simulation.links);
    }
}