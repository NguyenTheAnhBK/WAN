using MentorAlgorithm.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorAlgorithm.Algorithm
{
    //tổng lưu lượng vào ra tại một nút chính là trọng số của nút đó
    public class Mentor
    {
        public int NumberOfNode { get; private set; }
        public List<Node> Nodes { get; set; }
        public int Capacity { get; set; }
        public int Threshold { get; set; } //W
        public double Radius { get; set; }
        public double UMin { get; set; } = 0.7;
        public double Alpha { get; set; } = 0.5;
        private double _alphaOfCost = 0.4;

        public double _maxCost = 0;

        public double[,] Costs { get; set; }
        public Dictionary<Tuple<Node, Node>, double> Traffics { get; set; } = new Dictionary<Tuple<Node, Node>, double>(); //Lưu lượng giữa 2 nút bất kỳ
        public Dictionary<Tuple<Node, Node>, double> _trafficBackbones { get; set; } //= new Dictionary<Tuple<Node, Node>, double>(); //Lưu lượng thực tế đi qua nút backbone
        public Dictionary<Node, List<Node>> _clusters { get; set; }// = new Dictionary<Node, List<Node>>(); // cluster là một cụm với node đầu tiên là backbone và các node truy cập
        public Dictionary<Node, Node> _backboneConnect { get; set; } //= new Dictionary<Node, Node>(); //cây prim-dijkstra với các nút backbone
        public List<Node> BackboneConnect { get; set; } //BackboneConnect for plotter
        public List<Node> Backbones { get; private set; } = new List<Node>();
        //public Dictionary<Node, Node> Access { get; private set; } = new Dictionary<Node, Node>();
        public List<Node> Access { get; private set; } = new List<Node>(); //Access[chẵn] là backbone, Access[lẻ] là access
        public Dictionary<Tuple<Node, Node>, double> d { get; set; } // ma trận khoảng cách decac giữa các nút backbone
        public Dictionary<Tuple<Node, Node>, double> newCost { get; set; } // giá liên kết sau khi thêm liên kết trực tiếp
        public string OldD { get; set; } // ma trận khoảng cách trên cây giữa các nút backbone trước khi sử dụng liên kết trực tiếp Mentor 2
        public Dictionary<Tuple<Node, Node>, double> D { get; set; } //= new Dictionary<Tuple<Node, Node>, double>(); // ma trận khoảng cách trên cây giữa các nút backbone
        public List<Node> CenterBackbone { get; set; } //= new List<Node>(); //Node backbone trung tâm (để dưới dạng list để hiển thị lên plotter)
        public Dictionary<Node, Node> _addLinks { get; set; } = new Dictionary<Node, Node>(); //thêm liên kết trực tiếp
        public List<Node> AddLinks { get; set; } = new List<Node>(); //thêm liên kết trực tiếp cho plotter
        public Dictionary<Tuple<Node, Node>, Tuple<double, double>> LinksResult = new Dictionary<Tuple<Node, Node>, Tuple<double, double>>(); //số đường sử dụng và độ sử dụng của từng liên kết
        public List<Node> TreeLinks { get; set; } = new List<Node>();

        public Mentor(int n, int capacity, int threshold, double radius, double alpha, double umin)
        {
            NumberOfNode = n;
            Capacity = capacity;
            Threshold = threshold;
            Radius = radius;
            Alpha = alpha;
            UMin = umin;
        }

        public void SetTraffic(Node source, Node dest, int traffic)
        {
            source.Traffic += traffic;
            dest.Traffic += traffic;
            var t = Tuple.Create(source, dest);
            if (Traffics.ContainsKey(t))
                Traffics[t] += traffic;
            else
                Traffics.Add(Tuple.Create(source, dest), traffic);
        }

        //Generate random nodes
        public void GenerateNodes(int seed = 0)
        {
            Random random = new Random(DateTime.Now.Second);
            Nodes = new List<Node>();
            for (int i = 0; i < NumberOfNode; i++)
            {
                int randX = random.Next(0, NumberOfNode + 100);
                int randY = random.Next(0, NumberOfNode + 100);
                Nodes.Add(new Node(randX, randY, i.ToString()));
            }
        }

        //Generate Cost by Multiple Decac
        public void GenerateCosts(double alpha)
        {
            _alphaOfCost = alpha;
            //O(n^2)
            Costs = new double[NumberOfNode, NumberOfNode];
            for (int i = 0; i < NumberOfNode - 1; i++)
                for (int j = i; j < NumberOfNode; j++)
                {
                    double dis = Distance(Nodes[i], Nodes[j]);
                    if (dis > _maxCost)
                        _maxCost = dis;
                    Costs[i, j] = Costs[j, i] = dis * alpha;
                }
        }

        public static double Distance(Node a, Node b)
        {
            if (a == null || b == null)
                return 0;
            double dx = a.X - b.X, dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public void FindBackbone(Action<List<Node>> action = null)
        {
            //Xác định nút backbone trong mạng
            for (int i = 0; i < NumberOfNode; i++)
                if ((double)Nodes[i].Traffic / Capacity > Threshold)
                    Backbones.Add(Nodes[i]);

            _clusters = new Dictionary<Node, List<Node>>();
            //Xác định nút truy cập từ nút backbone đã xác định phía trước
            for(int i = 0; i < Backbones.Count; i++)
            {
                action?.Invoke(Access);
                FindAccessNode(Backbones[i]);
            }

            //Xác định nút backbone và nút truy nhập với những nút còn lại
            //có một vấn để là lưu lượng qua các nút còn lại = 0 thì không tính được merit => để nút đó tự do
            while (true)
            {
                List<Node> remainNodes = Nodes.Where(x => !x.Status && x.Traffic > 0).ToList();
                if (remainNodes.Count == 0)
                    break;
                //int totalTraffic = remainNodes.Sum(x => x.Traffic);
                //double xCentrol = remainNodes.Sum(node => node.X * node.Traffic) / totalTraffic;
                //double yCentrol = remainNodes.Sum(node => node.Y * node.Traffic) / totalTraffic;
                int totalTraffic = 0, maxTraffic = 0;
                double temp1 = 0, temp2 = 0;
                for (int i = 0; i < remainNodes.Count; i++)
                {
                    temp1 += remainNodes[i].X * remainNodes[i].Traffic;
                    temp2 += remainNodes[i].Y * remainNodes[i].Traffic;
                    totalTraffic += remainNodes[i].Traffic;
                    if (remainNodes[i].Traffic > maxTraffic)
                        maxTraffic = remainNodes[i].Traffic;
                }

                Node centrol = new Node(temp1 / totalTraffic, temp2 / totalTraffic, "Centrol");

                double[] di = new double[remainNodes.Count];
                double[] meriti = new double[remainNodes.Count];
                double maxDistanceCentrol = 0;
                for (int i = 0; i < remainNodes.Count; i++)
                {
                    double dis = Distance(remainNodes[i], centrol);
                    di[i] = dis;
                    if (dis > maxDistanceCentrol)
                        maxDistanceCentrol = dis;
                }

                //Hàm thưởng merit
                double maxMerit = 0;
                int index = 0;
                for (int i = 0; i < remainNodes.Count; i++)
                {
                    meriti[i] = 0.5 * (1 - di[i] / maxDistanceCentrol) + 0.5 * (remainNodes[i].Traffic / maxTraffic);
                    if (meriti[i] > maxMerit)
                    {
                        maxMerit = meriti[i];
                        index = i;
                    }
                }
                Backbones.Add(remainNodes[index]);
                action?.Invoke(Access);
                FindAccessNode(remainNodes[index]);
            }
        }

        private void FindAccessNode(Node backbone)
        {
            double RM = Radius * _maxCost;
            backbone.Status = true;
            if(!_clusters.Keys.Contains(backbone))
                _clusters.Add(backbone, new List<Node>());
            for (int j = 0; j < NumberOfNode; j++)
            {
                if (!Nodes[j].Status && Distance(backbone, Nodes[j]) <= RM)
                {
                    _clusters[backbone].Add(Nodes[j]);
                    //Access.Add(Nodes[j], Backbones[i]);
                    Access.Add(backbone);
                    Access.Add(Nodes[j]);
                    Nodes[j].Status = true;
                }
            }
        }

        //Chuyển lưu lượng giữa các nút backbones
        private double Traffic2Backbones(Node b1, Node b2)
        {
            double traffic = 0;
            List<Node> s = _clusters[b1];
            s.Add(b1);
            List<Node> d = _clusters[b2];
            d.Add(b2);

            for (int i = 0; i < s.Count; i++)
                for(int j = 0; j < d.Count; j++)
                    if (Traffics.ContainsKey(Tuple.Create(s[i], d[j])))
                        traffic += Traffics[Tuple.Create(s[i], d[j])];
            return traffic;
        }

        private Dictionary<Tuple<Node, Node>, double> RealTrafficBackbones()
        {
            _trafficBackbones = new Dictionary<Tuple<Node, Node>, double>();
            for (int i = 0; i < Backbones.Count; i++)
                for (int j = 0; j < Backbones.Count; j++)
                    if (i != j)
                    {
                        double traffic = Traffic2Backbones(Backbones[i], Backbones[j]);
                        if(traffic > 0)
                            if(!_trafficBackbones.Keys.Contains(Tuple.Create(Backbones[i], Backbones[j])))
                                _trafficBackbones.Add(Tuple.Create(Backbones[i], Backbones[j]), traffic);
                    }
            return _trafficBackbones;
        }

        //Tính ma trận khoảng cách decac giữa các nút backbone
        private void CalDistance()
        {
            d = new Dictionary<Tuple<Node, Node>, double>(Backbones.Count * Backbones.Count);
            newCost = new Dictionary<Tuple<Node, Node>, double>(Backbones.Count * Backbones.Count);
            for(int i = 0; i < Backbones.Count; i++)
            {
                if (!d.Keys.Contains(Tuple.Create(Backbones[i], Backbones[i])))
                {
                    d.Add(Tuple.Create(Backbones[i], Backbones[i]), 0);
                    newCost.Add(Tuple.Create(Backbones[i], Backbones[i]), 0);
                }
                for(int j = i + 1; j < Backbones.Count; j++)
                {
                    double dis = Distance(Backbones[i], Backbones[j]);
                    if(!d.Keys.Contains(Tuple.Create(Backbones[i], Backbones[j])))
                    {
                        d.Add(Tuple.Create(Backbones[i], Backbones[j]), dis);
                        newCost.Add(Tuple.Create(Backbones[i], Backbones[j]), dis); //
                    }
                    if (!d.Keys.Contains(Tuple.Create(Backbones[j], Backbones[i])))
                    {
                        d.Add(Tuple.Create(Backbones[j], Backbones[i]), dis);
                        newCost.Add(Tuple.Create(Backbones[j], Backbones[i]), dis); //
                    }
                }
            }
        }

        //Lựa chọn nút trung tâm trong số các nút backbone
        private Node FindCenterBackbone()
        {
            double momeni, minMomen = double.MaxValue;
            Node centerBackbone = new Node(0, 0, "Null");
            CenterBackbone = new List<Node>();
            CalDistance();
            for(int i = 0; i < Backbones.Count; i++)
            {
                momeni = 0;
                for(int j = 0; j < Backbones.Count; j++)
                    momeni += d[Tuple.Create(Backbones[i], Backbones[j])] * Backbones[j].Traffic;
                if (momeni < minMomen)
                {
                    minMomen = momeni;
                    centerBackbone = Backbones[i];
                }
            }
            CenterBackbone.Add(centerBackbone);
            return centerBackbone;
        }

        //Kết nối các nút backbone với nhau sử dụng cây Prim-Dijkstra
        public void ConnectBackbone()
        {
            Node centerBB = FindCenterBackbone();
            //Dijkstra dijkstra = new Dijkstra(d, Backbones, Backbones.Count);//

            RealTrafficBackbones();
            //PrimDijkstra primDijkstra = new PrimDijkstra(_trafficBackbones, Backbones, Backbones.Count, Alpha);//
            PrimDijkstra primDijkstra = new PrimDijkstra(d, Backbones, Backbones.Count, Alpha);
            Dictionary<Node, Node> path = primDijkstra.FindPath(centerBB);
            Dictionary<Tuple<Node, Node>, int> edges = new Dictionary<Tuple<Node, Node>, int>(); //chuyển từ tree (path) sang edge
            foreach(var p in path)
            {
                edges.Add(Tuple.Create(p.Key, p.Value), 0);
                edges.Add(Tuple.Create(p.Value, p.Key), 0);
            }
            _backboneConnect = path;

            //convert _backboneConnect to BackboneConnect để hiện thị lên plotter
            //BackboneConnect = new List<Node>();
            //foreach(var p in path)
            //{
            //    BackboneConnect.Add(p.Key);
            //    BackboneConnect.Add(p.Value);
            //    BackboneConnect.Add(new Node(double.NaN, double.NaN, "Null"));
            //}
            BackboneConnect = ConvertDisplayPlotter(_backboneConnect);

            //Tính ma trận khoảng cách trên cây giữa các nút backbone
            //Nếu là cây thì ta có khoảng cách giữa 2 nút bất kỳ sẽ là: Dist(n1, n2) = Dist(root, n1) + Dist(root, n2) - 2*Dist(root, lca) 
            //với lcs là lowest common ancestor (https://www.geeksforgeeks.org/find-distance-between-two-nodes-of-a-binary-tree/)

            //Node lca = LowestCommonAncestor(centerBB, Backbones[1], Backbones[2], _backboneConnect);

            //Đường đi từ Node a tới Node b
            D = new Dictionary<Tuple<Node, Node>, double>();
            Dictionary<Tuple<Node, Node>, List<Node>> nodeOnPath = new Dictionary<Tuple<Node, Node>, List<Node>>();
            for (int i = 0; i < Backbones.Count; i++)
            {
                if(!D.Keys.Contains(Tuple.Create(Backbones[i], Backbones[i])))
                    D.Add(Tuple.Create(Backbones[i], Backbones[i]), 0);
                for (int j = i + 1; j < Backbones.Count; j++)
                {
                    var t1 = Tuple.Create(Backbones[i], Backbones[j]);
                    var t2 = Tuple.Create(Backbones[j], Backbones[i]);
                    double dOnTree = 0;
                    if(!nodeOnPath.Keys.Contains(t1))
                        nodeOnPath.Add(t1, new List<Node>());
                    if(!nodeOnPath.Keys.Contains(t2))
                        nodeOnPath.Add(t2, new List<Node>());
                    //nodeOnPath[t1] = nodeOnPath[t2] = DistanceOnTree(centerBB, Backbones[i], Backbones[j], _backboneConnect, ref dOnTree);
                    nodeOnPath[t1] = nodeOnPath[t2] = DistanceOnGraph(Backbones[i], Backbones[j], edges, Backbones, ref dOnTree);
                    if(!D.Keys.Contains(t1))
                        D.Add(t1, dOnTree);
                    if(!D.Keys.Contains(t2))
                        D.Add(t2, dOnTree);
                }
            }

            OldD = D.ToStringTable("Node", Backbones, node => node.Name);

            IncrementalShortestPath(d, D, Backbones, _trafficBackbones, nodeOnPath, Capacity, UMin, _addLinks, LinksResult, newCost);

            //BackboneConnect = ConvertDisplayPlotter(_backboneConnect);
            AddLinks = ConvertDisplayPlotter(_addLinks);
        }

        List<Node> ConvertDisplayPlotter(Dictionary<Node, Node> connect)
        {
            List<Node> plotter = new List<Node>();
            foreach (var p in connect)
            {
                plotter.Add(p.Key);
                plotter.Add(p.Value);
                plotter.Add(new Node(double.NaN, double.NaN, "Null"));
            }
            return plotter;
        }

        static void DFS(Node curr, Node prev, Node u, Dictionary<Node, Node> tree, ref Node[,] path, int numberPath, int level, ref bool flag)
        {
            if (curr == u)
                return;
            foreach(var edge in tree){
                if(edge.Value == curr && edge.Key != prev && !flag)
                {
                    path[numberPath, level] = edge.Key;
                    if (edge.Key == u)
                    {
                        flag = true;
                        //path[numberPath, level + 1] = null;
                        return;
                    }
                    
                    DFS(edge.Key, curr, u, tree, ref path, numberPath, level + 1, ref flag);
                }
            }
        }

        public static Node LowestCommonAncestor(Node root, Node u, Node v, Dictionary<Node, Node> edges)
        {
            if (u == v)
                return u;

            Node[,] path = new Node[2, edges.Count];
            bool flag = false;

            path[0, 0] = path[1, 0] = new Node(-1, -1, "Null");

            //path root -> u
            DFS(root, new Node(-1, -1, "Null"), u, edges, ref path, 0, 1, ref flag);
            //path root -> v
            flag = false;
            DFS(root, new Node(-1, -1, "Null"), v, edges, ref path, 1, 1, ref flag);

            int i = 0;
            while (path[0, i] != null && path[1, i] != null && path[0, i] == path[1, i])
                i++;
            return path[0, i - 1];
        }

        public static List<Node> DistanceOnTree(Node root, Node u, Node v, Dictionary<Node, Node> tree, ref double d)
        {
            if (u == v)
                return new List<Node>();

            Node[,] path = new Node[2, tree.Count];
            bool flag = false;

            path[0, 0] = path[1, 0] = root;

            //path root -> u
            DFS(root, new Node(-1, -1, "Null"), u, tree, ref path, 0, 1, ref flag);
            //path root -> v
            flag = false;
            DFS(root, new Node(-1, -1, "Null"), v, tree, ref path, 1, 1, ref flag);

            double dRootToU = 0, dRootToV = 0, dRootToLca = 0;
            List<Node> nodeOnPath = new List<Node>();

            int i = 0;
            while (path[0, i] != null && i < tree.Count - 1)
            {
                nodeOnPath.Add(path[0, i]);
                dRootToU += Distance(path[0, i], path[0, ++i]);
            }
                
            i = 0;
            while (path[1, i] != null && i < tree.Count - 1)
            {
                nodeOnPath.Add(path[1, i]);
                dRootToV += Distance(path[1, i], path[1, ++i]);
            }

            i = 0;
            while (path[0, i] != null && path[1, i] != null && path[0, i] == path[1, i] && i < tree.Count - 1)
            {
                //nodeOnPath.Remove(path[0, i]);
                nodeOnPath.Remove(path[0, i]);
                dRootToLca += Distance(path[0, i], path[0, ++i]);
            }
                
            dRootToLca -= Distance(path[0, i], path[0, --i]);
            nodeOnPath.Remove(u);
            nodeOnPath.Remove(v);

            d = dRootToU + dRootToV - 2 * dRootToLca;
            return nodeOnPath;
        }

        public static List<Node> DistanceOnGraph(Node u, Node v, Dictionary<Tuple<Node, Node>, int> graph, List<Node> nodes, ref double d)
        {
            if (u == v)
                return new List<Node>();

            List<Node> path = new List<Node>(nodes.Count);
            Dictionary<Node, bool> visited = new Dictionary<Node, bool>();
            bool flag = false;
            for (int i = 0; i < nodes.Count; i++)
                visited[nodes[i]] = false;

            visited[u] = true;
            path.Add(u);
            DFSGraph(u, v, graph, nodes, ref flag, ref visited, ref path);

            for(int i = 0; i < path.Count - 1; i++)
                d += Distance(path[i], path[i + 1]);

            path.Remove(u);
            path.Remove(v);
            return path; //node on path
        }

        public static void DFSGraph(Node u, Node v, Dictionary<Tuple<Node, Node>, int> graph, List<Node> nodes, ref bool flag, ref Dictionary<Node, bool> visited, ref List<Node> path)
        {
            if (u == v)
            {
                flag = true;
                return;
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                if(!flag && graph.ContainsKey(Tuple.Create(u, nodes[i])) && !visited[nodes[i]])
                {
                    visited[nodes[i]] = true;
                    path.Add(nodes[i]);
                    DFSGraph(nodes[i], v, graph, nodes, ref flag, ref visited, ref path);
                    if(!flag)
                        path.Remove(nodes[i]);
                }
            }
        }

        public static Node GetHome(Dictionary<Tuple<Node, Node>, double> d, Node u, Node v, List<Node> nodeOnPath)
        {
            Node home = null;
            double dMin = double.MaxValue;
            for (int i = 0; i < nodeOnPath.Count; i++)
            {
                if (nodeOnPath[i]?.Name != null)
                {
                    double distance = d[Tuple.Create(nodeOnPath[i], u)] + d[Tuple.Create(nodeOnPath[i], v)];
                    if (distance < dMin)
                    {
                        dMin = distance;
                        home = nodeOnPath[i];
                    }
                }
            }
            return home;
        }

        public static void IncrementalShortestPath(Dictionary<Tuple<Node, Node>, double> d, Dictionary<Tuple<Node, Node>, double> D, List<Node> backbones, Dictionary<Tuple<Node, Node>, double> traffics, Dictionary<Tuple<Node, Node>, List<Node>> nodeOnPath, double C, double uMin, Dictionary<Node, Node> addLinks = null, Dictionary<Tuple<Node, Node>, Tuple<double, double>> linksResult = null, Dictionary<Tuple<Node, Node>, double> newCost = null)
        {
            Dictionary<Tuple<Node, Node>, int> links = new Dictionary<Tuple<Node, Node>, int>(); //Liên kết n hops
            foreach (var traffic in traffics)
            {
                links.Add(traffic.Key, nodeOnPath[traffic.Key].Count + 1);
                if (linksResult != null)
                    linksResult.Add(traffic.Key, Tuple.Create(.0, .0));
            }
                

            while (links.Count > 0)
            {
                links = links.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                var link = links.First();
               
                double n = Math.Ceiling(traffics[link.Key] / C);
                double u = traffics[link.Key] / (C * n);
                if (linksResult != null)
                    linksResult[link.Key] = Tuple.Create(n, u);

                 if (link.Value < 2)
                    return;
                if (u < uMin)
                {
                    var home = Mentor.GetHome(d, link.Key.Item1, link.Key.Item2, nodeOnPath[link.Key]);
                    if (home?.Name != null)
                    {
                        var t1 = Tuple.Create(link.Key.Item1, home);
                        if (!traffics.ContainsKey(t1))
                        {
                            traffics.Add(t1, 0);
                            links.Add(t1, nodeOnPath[t1].Count + 1);
                        }
                        traffics[t1] += traffics[link.Key];

                        var t2 = Tuple.Create(home, link.Key.Item2);
                        if (!traffics.ContainsKey(t2))
                        {
                            traffics.Add(t2, 0);
                            links.Add(t2, nodeOnPath[t2].Count + 1);
                        }
                        traffics[t2] += traffics[link.Key];
                    }
                }
                else
                {
                    List<Node> sList = new List<Node>();
                    List<Node> dList = new List<Node>();
                    List<Node> consider = backbones.Except(nodeOnPath[link.Key]).ToList();
                    double L = d[link.Key]; //Khoảng cách decac
                    foreach (var node in consider)
                    {
                        double dNodeToS = D[Tuple.Create(node, link.Key.Item1)];
                        double dNodeToD = D[Tuple.Create(node, link.Key.Item2)];
                        if (dNodeToS + L < dNodeToD)
                            sList.Add(node);
                        else if (dNodeToD + L < dNodeToS)
                            dList.Add(node);
                    }

                    Dictionary<Tuple<Node, Node>, double> maxL = new Dictionary<Tuple<Node, Node>, double>();
                    for (int i = 0; i < sList.Count; i++)
                    {
                        for (int j = 0; j < dList.Count; j++)
                        {
                            double l = D[Tuple.Create(sList[i], dList[j])] - D[Tuple.Create(sList[i], link.Key.Item1)] - D[Tuple.Create(link.Key.Item2, dList[j])];
                            maxL.Add(Tuple.Create(sList[i], dList[j]), l);
                        }
                    }
                    double max = 0;
                    if (maxL.Count > 0)
                        max = maxL.Where(l => l.Key != link.Key).Max(x => x.Value);

                    int newd = (int)max + 1;
                    foreach (var sd in maxL)
                    {
                        if (sd.Value > L)
                        {
                            //D[sd.Key] = sd.Value;
                            //D[Tuple.Create(sd.Key.Item2, sd.Key.Item1)] = sd.Value;
                            //D[link.Key] = d[link.Key];

                            D[sd.Key] = newd;
                            D[Tuple.Create(sd.Key.Item2, sd.Key.Item1)] = newd;
                            D[link.Key] = d[link.Key];

                            if (newd > d[sd.Key])
                                newCost[sd.Key] = newd;
                            //if(backboneConnect != null)
                            //{
                            //    backboneConnect[link.Key.Item2] = link.Key.Item1;
                            //    foreach (var node in nodeOnPath[link.Key])
                            //        if(backboneConnect.ContainsKey(node) && backboneConnect[node] == link.Key.Item1)
                            //            backboneConnect.Remove(node);
                            //}
                        }
                    }
                    //thêm liên kết trực tiếp
                    if (addLinks != null)
                        addLinks[link.Key.Item2] = link.Key.Item1;
                }
                links.Remove(link.Key);
            }
        }
    }
}
