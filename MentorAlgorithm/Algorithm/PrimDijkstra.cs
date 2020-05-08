using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentorAlgorithm.Algorithm
{
    public class PrimDijkstra
    {
        public const double INF = double.MaxValue;
        public int V { get; set; } // V is Vertices: số đỉnh
        //public Node[,] G { get; private set; }
        public double Alpha { get; set; } = 0.3;
        public Dictionary<Tuple<Node, Node>, double> Edges { get; set; }
        public List<Node> Nodes { get; set; }

        public PrimDijkstra(Dictionary<Tuple<Node, Node>, double> edges, List<Node> nodes, int vertices, double alpha)
        {
            V = vertices;
            Nodes = nodes;
            Edges = edges;
            Alpha = alpha;
        }

        public PrimDijkstra(List<Edge> edges, List<Node> nodes, int vertices, double alpha)
        {
            //G = new Node[vertices, vertices];
            //Edges = edges;
            V = vertices;
            Nodes = nodes;
            Alpha = alpha;
            Edges = new Dictionary<Tuple<Node, Node>, double>(vertices * vertices);
            //for(int i = 0; i < vertices; i++)
            //    for(int j = 0; j < vertices; j++)
            //        Edges.con
            for (int i = 0; i < edges.Count; i++)
            {
                Edges.Add(Tuple.Create(edges[i].Source, edges[i].Destination), edges[i].Cost);
                Edges.Add(Tuple.Create(edges[i].Destination, edges[i].Source), edges[i].Cost);
            }
        }

        public void AddEdge(Node s, Node d, int w)
        {
            //G[u, v] = G[v, u] = w;
            //Edges.Add(new Edge { Source = s, Destination = d, Cost = w });
            Edges.Add(Tuple.Create(s, d), w);
            Edges.Add(Tuple.Create(d, s), w);
        }

        public Dictionary<Node, Node> FindPath(Node s) // s là đỉnh gốc
        {
            Dictionary<Node, double> d = new Dictionary<Node, double>(V); // nhãn của nút
            Dictionary<Node, double> L = new Dictionary<Node, double>(V); // nhãn của nút theo prim-dijkstra
            Dictionary<Node, bool> visited = new Dictionary<Node, bool>(V);
            Dictionary<Node, Node> p = new Dictionary<Node, Node>(V); // p[u] = v tức là u là con của v

            for (int i = 0; i < Nodes.Count; i++)
            {
                d[Nodes[i]] = INF;
                L[Nodes[i]] = INF;
                visited[Nodes[i]] = false;
                //p[Nodes[i]] = s;
            }

            d[s] = 0;
            L[s] = 0;

            int count = 1;
            double lMin;
            Node u = s;
            while (count < V)
            {
                lMin = INF;
                //Tìm node có nhãn min
                for (int i = 0; i < V; i++)
                {
                    if (!visited[Nodes[i]] && L[Nodes[i]] < lMin)
                    {
                        lMin = L[Nodes[i]];
                        u = Nodes[i];
                    }
                }

                //Xét node u
                visited[u] = true;
                for (int i = 0; i < V; i++)
                {
                    Tuple<Node, Node> t = Tuple.Create(u, Nodes[i]);
                    if (!visited[Nodes[i]] && Edges.ContainsKey(t) && Alpha * d[u] + Edges[t] < L[Nodes[i]])
                    {
                        d[Nodes[i]] = d[u] + Edges[t];
                        L[Nodes[i]] = Alpha * d[u] + Edges[t];
                        p[Nodes[i]] = u;
                    }
                }
                count++;
            }
            return p;
        }
    }
}
