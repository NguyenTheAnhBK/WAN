﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MentorAlgorithm.Algorithm
{
    //Tìm đường đi ngắn nhất từ một điểm đến tất cả điểm còn lại trong đồ thị
    public class Dijkstra
    {
        public const double INF = double.MaxValue;
        public int V { get; set; } // V is Vertices: số đỉnh
        //public Node[,] G { get; private set; }
        public Dictionary<Tuple<Node, Node>, double> Edges { get; set; }
        public List<Node> Nodes { get; set; }

        public Dijkstra(Dictionary<Tuple<Node, Node>, double> edges, List<Node> nodes, int vertices)
        {
            V = vertices;
            Nodes = nodes;
            Edges = edges;
        }

        public Dijkstra(List<Edge> edges, List<Node> nodes, int vertices)
        {
            //G = new Node[vertices, vertices];
            //Edges = edges;
            V = vertices;
            Nodes = nodes;
            Edges = new Dictionary<Tuple<Node, Node>, double>(vertices * vertices);
            //for(int i = 0; i < vertices; i++)
            //    for(int j = 0; j < vertices; j++)
            //        Edges.con
            for(int i = 0; i < edges.Count; i++)
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
            Dictionary<Node, bool> visited = new Dictionary<Node, bool>(V);
            Dictionary<Node, Node> p = new Dictionary<Node, Node>(V); // p[u] = v tức là u là con của v

            for (int i = 0; i < Nodes.Count; i++)
            {
                d[Nodes[i]] = INF;
                visited[Nodes[i]] = false;
                //p[Nodes[i]] = s;
            }

            d[s] = 0;

            int count = 1;
            double dMin;
            Node u = s;
            while (count < V)
            {
                dMin = INF;
                //Tìm node có nhãn min
                for (int i = 0; i < V; i++)
                {
                    if (!visited[Nodes[i]] && d[Nodes[i]] < dMin)
                    {
                        dMin = d[Nodes[i]];
                        u = Nodes[i];
                    }
                }

                visited[u] = true;
                //Xét node u
                for (int i = 0; i < V; i++)
                {
                    Tuple<Node, Node> t = Tuple.Create(u, Nodes[i]);
                    if (!visited[Nodes[i]] && Edges.ContainsKey(t) && d[u] + Edges[t] < d[Nodes[i]])
                    {
                        d[Nodes[i]] = d[u] + Edges[t];
                        p[Nodes[i]] = u;
                    }
                }
                count++;
            }
            return p;
        }

        //public List<int> FindPath(int s) // s là đỉnh gốc
        //{
        //    List<int> d = Enumerable.Repeat(INF, V).ToList();
        //    List<bool> visited = Enumerable.Repeat(false, V).ToList();
        //    List<int> p = Enumerable.Repeat(s, V).ToList(); // p[u] = v tức là u là con của v

        //    d[s] = 0;

        //    int count = 1, u = 0, dMin;
        //    while(count < V)
        //    {
        //        dMin = INF;
        //        //Tìm đỉnh nhãn min
        //        for(int v = 0; v < V; v++)
        //        {
        //            if(d[v] < dMin)
        //            {
        //                dMin = d[v];
        //                u = v;
        //            }
        //        }

        //        //Xét đỉnh u
        //        for(int v = 0; v < V; v++)
        //        {
        //            if(!visited[v] && d[u] + G[u, v] < d[v])
        //            {
        //                d[v] = d[u] + G[u, v];
        //                visited[v] = true;
        //                p[v] = u;
        //            }
        //        }
        //        count++;
        //    }
        //    return p;
        //}

        //Use priority queue
    }
}
