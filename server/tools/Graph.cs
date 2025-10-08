using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace server.tools
{
    public class Graph
    {
        private Dictionary<int, List<Tuple<int, int>>> adjacencyList;
        private HashSet<int> visited;
        public Graph()
        {
            adjacencyList = [];
            visited = [];
        }

        public void AddNode(int vertex)
        {
            if (vertex <= 0)
            {
                Console.WriteLine("Only positive integers allowed");
            }
            else if (!adjacencyList.ContainsKey(vertex))
            {
                adjacencyList[vertex] = [];
            }
        }
        public void AddLink(int start, int end, int cost)
        {
            if (cost < 0) return;
            if (!adjacencyList.ContainsKey(start) || !adjacencyList.ContainsKey(end))
            {
                Console.WriteLine("Referenced nodes are not available");
            }
            else
            {
                var value = adjacencyList[start].Find((Tuple<int, int> val) => val.Item1 == end);
                if (value != null)
                {
                    Console.WriteLine("This link already exists");
                    Console.WriteLine("So I'll just update the cost");
                    Tuple<int, int> tuple = new Tuple<int, int>(value.Item1, cost);
                    adjacencyList[start].Remove(value);
                    adjacencyList[start].Add(tuple);
                    return;
                }
                adjacencyList[start].Add(new Tuple<int, int>(end, cost));
            }
        }
        private void DFS(int startNode, int end)
        {
            if (!adjacencyList.ContainsKey(startNode)) return;
            if (visited.Contains(startNode)) return;
            Console.WriteLine(startNode);
            visited.Add(startNode);
            if (startNode == end) return;
            foreach (Tuple<int, int> val in adjacencyList[startNode])
            {
                DFS(val.Item1, end);
            }
        }
        public void DFSHelper(int startNode, int endNode = -1)
        {
            visited.Clear();
            DFS(startNode, endNode);
        }
        public void BFS(int start, int end = -1)
        {
            if (!adjacencyList.ContainsKey(start) || !adjacencyList.ContainsKey(end)) return;

            Queue<int> processQueue = new Queue<int>();

            // Clear visited hash set
            visited.Clear();

            processQueue.Enqueue(start);
            visited.Add(start);

            while (processQueue.Count > 0)
            {
                int current = processQueue.Dequeue();
                Console.WriteLine(current);

                foreach (Tuple<int, int> neighbor in adjacencyList[current])
                {
                    if (!visited.Contains(neighbor.Item1))
                    {
                        if (neighbor.Item1 == end)
                        {
                            return;
                        }
                        processQueue.Enqueue(neighbor.Item1);
                        visited.Add(neighbor.Item1);
                    }
                }
            }
        }

        // Single point shortest path algorithm
        public string DijkstraAlgorithm(int start, List<string> mappings)
        {
            string output = "";
            if (!adjacencyList.ContainsKey(start))
            {
                return "This starting point is not a part of this graph";
            }
            
            var dist = new Dictionary<int, int>();
            var prev = new Dictionary<int, int>();
            foreach (var key in adjacencyList.Keys)
            {
                dist[key] = int.MaxValue;
                prev[key] = -1;
            }

            dist[start] = 0;

            // min-heap of (node, currentDistance)
            Heap pq = new Heap();
            pq.Add(Tuple.Create(start, 0));

            while (!pq.IsEmpty())
            {
                var currentNode = pq.Pop();
                if (currentNode == null) break;

                int currentValue = currentNode.Item1;
                int currentCost = currentNode.Item2;

                if (currentCost > dist[currentValue]) continue;

                // Check all the children of the current node
                foreach (var edge in adjacencyList[currentValue])
                {
                    int vertex = edge.Item1;
                    int weight = edge.Item2;
                    // Check if you have a better/cheaper link than the existing one to your neighbours
                    long ndLong = currentCost + weight;
                    if (ndLong < dist[vertex])
                    {
                        // If so, we update the distance array
                        dist[vertex] = (int)ndLong;
                        // And the previous array
                        prev[vertex] = currentValue;
                        pq.Add(Tuple.Create(vertex, dist[vertex]));
                    }
                }
            }

            // Output distances (or use prev[] to reconstruct paths)
            Console.WriteLine($"Shortest distances from node {start}:");
            foreach (var kvp in dist)
            {
                string nodeName = mappings[kvp.Key];
                string distance = kvp.Value == int.MaxValue ? "∞" : kvp.Value.ToString();
                string path = "";
                int currentNode = kvp.Key;
                // Reconstruct path from start to currentNode
                var pathNodes = new List<string>();
                if (kvp.Value != int.MaxValue)
                {
                    while (currentNode != -1)
                    {
                        pathNodes.Add(mappings[currentNode]);
                        currentNode = prev[currentNode];
                    }
                    pathNodes.Reverse();
                    path = string.Join(" -> ", pathNodes);
                }
                output += $"{nodeName}: {distance}, Path: {path}\n";
            }
            
            return output;
        }
    }
}
