using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorAlgorithm.Algorithm
{
    public class Mentor
    {
        public int NumberOfNode { get; private set; }
        public List<Node> Nodes { get; set; }
        public int Capacity { get; set; }
        public int Threshold { get; set; } //W
        public double Radius { get; set; }

        private double _maxCost = 0;

        public double[,] Costs { get; set; }
        public List<Node> Backbones { get; private set; } = new List<Node>();
        //public Dictionary<Node, Node> Access { get; private set; } = new Dictionary<Node, Node>();
        public List<Node> Access { get; private set; } = new List<Node>();

        public Mentor(int n, int capacity, int threshold, double radius)
        {
            NumberOfNode = n;
            Capacity = capacity;
            Threshold = threshold;
            Radius = radius;
        }

        public void SetTraffic(Node source, Node dest, int traffic)
        {
            source.Traffic += traffic;
            dest.Traffic += traffic;
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
            double dx = a.X - b.X, dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public void FindBackbone(Action<List<Node>> action = null)
        {
            //Xác định nút backbone trong mạng
            for (int i = 0; i < NumberOfNode; i++)
                if (Nodes[i].Traffic / Capacity > Threshold)
                    Backbones.Add(Nodes[i]);

            //Xác định nút truy cập từ nút backbone đã xác định phía trước
            for(int i = 0; i < Backbones.Count; i++)
            {
                action?.Invoke(Access);
                FindAccessNode(Backbones[i]);
            }

            //Xác định nút backbone và nút truy nhập với những nút còn lại
            while (true)
            {
                List<Node> remainNodes = Nodes.Where(x => !x.Status).ToList();
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
            for (int j = 0; j < NumberOfNode; j++)
            {
                if (!Nodes[j].Status && Distance(backbone, Nodes[j]) <= RM)
                {
                    //Access.Add(Nodes[j], Backbones[i]);
                    Access.Add(backbone);
                    Access.Add(Nodes[j]);
                    Nodes[j].Status = true;
                }
            }
        }
    }
}
