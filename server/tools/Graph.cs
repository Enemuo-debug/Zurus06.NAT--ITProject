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
        private Dictionary<string, List<Tuple<string, int>>> adjacencyList;
        private HashSet<string> visited;
        public Graph()
        {
            adjacencyList = [];
            visited = [];
        }

        public bool AddNode(string vertex)
        {
            if (string.IsNullOrEmpty(vertex))
            {
                Console.WriteLine("Invalid Vertex");
                return false;
            }
            else if (!adjacencyList.ContainsKey(vertex))
            {
                adjacencyList[vertex] = [];
                return true;
            }
            else
            {
                Console.WriteLine("Vertex already exists");
                return false;
            }
        }

        public bool AddNodes (List<string> vertices)
        {
            foreach (var item in vertices)
            {
                bool success = AddNode(item);
                if (!success)
                {
                    Console.WriteLine("These nodes either contain duplicates or contain empty strings");
                    return false;
                }
            }
            return true;
        }

        public bool AddLink(string start, string end, int cost)
        {
            if (cost < 0) return false;
            if (!adjacencyList.ContainsKey(start) || !adjacencyList.ContainsKey(end))
            {
                Console.WriteLine("Referenced nodes are not available");
                return false;
            }
            else
            {
                var value = adjacencyList[start].Find((Tuple<string, int> val) => val.Item1 == end);
                if (value != null)
                {
                    Console.WriteLine("This link already exists");
                    Console.WriteLine("So I'll just update the cost");
                    Tuple<string, int> tuple = new(value.Item1, cost);
                    adjacencyList[start].Remove(value);
                    adjacencyList[start].Add(tuple);
                    return true;
                }
                adjacencyList[start].Add(new Tuple<string, int>(end, cost));
                return true;
            }
        }
        private void DFS(string startNode, string end)
        {
            if (!adjacencyList.ContainsKey(startNode)) return;
            if (visited.Contains(startNode)) return;
            visited.Add(startNode);
            if (startNode == end) return;
            foreach (Tuple<string, int> val in adjacencyList[startNode])
            {
                DFS(val.Item1, end);
            }
        }
        public void DFSHelper(string startNode, string endNode)
        {
            visited.Clear();
            DFS(startNode, endNode);
        }
        public void BFS(string start, string end)
        {
            if (!adjacencyList.ContainsKey(start) || !adjacencyList.ContainsKey(end)) return;

            Queue<string> processQueue = new Queue<string>();

            visited.Clear();

            processQueue.Enqueue(start);
            visited.Add(start);

            while (processQueue.Count > 0)
            {
                string current = processQueue.Dequeue();
                Console.WriteLine(current);

                foreach (Tuple<string, int> neighbor in adjacencyList[current])
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
        public string DijkstraAlgorithm(string start, out List<string> paths)
        {
            paths = new();
            string output = "";

            if (!adjacencyList.ContainsKey(start))
                return "This starting point is not in the graph.";

            var dist = new Dictionary<string, int>();
            var prev = new Dictionary<string, string>();

            foreach (var key in adjacencyList.Keys)
            {
                dist[key] = int.MaxValue;
                prev[key] = null;
            }

            dist[start] = 0;

            // priority queue
            Heap pq = new();
            pq.Add(Tuple.Create(start, 0));

            while (!pq.IsEmpty())
            {
                var currentNode = pq.Pop();
                if (currentNode == null) break;

                string u = currentNode.Item1;
                int currentDist = currentNode.Item2;

                if (currentDist > dist[u]) 
                    continue;

                foreach (var edge in adjacencyList[u])
                {
                    string v = edge.Item1;
                    int weight = edge.Item2;

                    long newDist = currentDist + weight;

                    if (newDist < dist[v])
                    {
                        dist[v] = (int)newDist;
                        prev[v] = u;
                        pq.Add(Tuple.Create(v, dist[v]));
                    }
                }
            }

            // Prepare output
            output+=$"Shortest paths from node {start}:";

            foreach (var node in adjacencyList.Keys)
            {
                if (dist[node] == int.MaxValue)
                {
                    output+=$"{node} : unreachable";
                    paths.Add($"{node}: unreachable");
                    continue;
                }

                // Reconstruct path
                List<string> pathList = new();
                string curr = node;

                while (curr != null)
                {
                    pathList.Add(curr);
                    curr = prev[curr];
                }

                pathList.Reverse();

                string pathString = string.Join("->", pathList);
                string distance = dist[node].ToString();

                output+=$"{node}: distance={distance}, path={pathString}";
                paths.Add(pathString);
            }

            return output;
        }
    }
}
