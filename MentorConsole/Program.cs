using MentorAlgorithm.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            double C = 12;
            double uMin = 0.8;
            List<Node> backbones = new List<Node>() {
                new Node(4, 1, "3"), //root
                new Node(9, 1, "4"),
                new Node(2, 3, "6"),
                new Node(6, 5, "8"),
                new Node(0, 6, "10"),
                new Node(8, 7, "13"),
                new Node(1, 8, "14")
            };

            Dictionary<Node, Node> tree = new Dictionary<Node, Node>();
            tree.Add(backbones[1], backbones[3]);
            tree.Add(backbones[2], backbones[0]);
            tree.Add(backbones[3], backbones[6]);
            tree.Add(backbones[4], backbones[2]);
            tree.Add(backbones[5], backbones[3]);
            tree.Add(backbones[6], backbones[4]);

            Dictionary<Tuple<Node, Node>, double> traffics = new Dictionary<Tuple<Node, Node>, double>();
            traffics.Add(Tuple.Create(backbones[0], backbones[1]), 5); //T(3, 4) = 5
            //traffics.Add(Tuple.Create(backbones[1], backbones[0]), 5);
            traffics.Add(Tuple.Create(backbones[2], backbones[1]), 6); //T(6, 4) = 6
            //traffics.Add(Tuple.Create(backbones[1], backbones[2]), 6);
            traffics.Add(Tuple.Create(backbones[0], backbones[3]), 10); //T(3, 8) = 10
            //traffics.Add(Tuple.Create(backbones[3], backbones[0]), 10);
            traffics.Add(Tuple.Create(backbones[4], backbones[1]), 6); //T(10, 4) = 6
            //traffics.Add(Tuple.Create(backbones[1], backbones[4]), 6); 

            //Ma trận khoảng cách trên cây
            Dictionary<Tuple<Node, Node>, double> D = new Dictionary<Tuple<Node, Node>, double>();
            //Ma trận khoảng cách decac
            Dictionary<Tuple<Node, Node>, double> d = new Dictionary<Tuple<Node, Node>, double>();
            //Đường đi từ Node a tới Node b
            Dictionary<Tuple<Node, Node>, List<Node>> nodeOnPath = new Dictionary<Tuple<Node, Node>, List<Node>>();

            for(int i = 0; i < backbones.Count; i++)
            {
                D.Add(Tuple.Create(backbones[i], backbones[i]), 0);
                d.Add(Tuple.Create(backbones[i], backbones[i]), 0);
                for(int j = i + 1; j < backbones.Count; j++)
                {
                    var t1 = Tuple.Create(backbones[i], backbones[j]);
                    var t2 = Tuple.Create(backbones[j], backbones[i]);
                    double dOnTree = 0, dDecac = 0;
                    nodeOnPath.Add(t1, new List<Node>());
                    nodeOnPath.Add(t2, new List<Node>());
                    nodeOnPath[t1] = nodeOnPath[t2] = Mentor.DistanceOnTree(backbones[0], backbones[i], backbones[j], tree, ref dOnTree);
                    D.Add(t1, dOnTree);
                    D.Add(t2, dOnTree);
                    dDecac = Mentor.Distance(backbones[i], backbones[j]);
                    d.Add(t1, dDecac);
                    d.Add(t2, dDecac);
                }
            }

            //Hiển thị ma trận khoảng cách trên cây
            Console.Write("Ma trận khoảng cách trên cây: \n");
            for (int i = 0; i < backbones.Count; i++)
                Console.Write("\t" + backbones[i].Name);
            Console.Write("\n");
            for(int i = 0; i < backbones.Count; i++)
            {
                Console.Write(backbones[i].Name + "\t");
                for (int j = 0; j < backbones.Count; j++)
                    Console.Write(D[Tuple.Create(backbones[i], backbones[j])].ToString("f2") + "\t");
                Console.Write("\n");
            }

            //Sắp xếp lưu lượng theo thứ tự giảm dần khoảng cách trên cây
            //var traffics = traffics.OrderByDescending(x => D[x.Key]);

            Mentor.IncrementalShortestPath(d, D, backbones, traffics, nodeOnPath, C, uMin);

            //Hiển thị ma trận khoảng cách trên cây sau giải thuật ISP
            Console.Write("\nMa trận khoảng cách trên cây sử dụng ISP: \n");
            for (int i = 0; i < backbones.Count; i++)
                Console.Write("\t" + backbones[i].Name);
            Console.Write("\n");
            for (int i = 0; i < backbones.Count; i++)
            {
                Console.Write(backbones[i].Name + "\t");
                for (int j = 0; j < backbones.Count; j++)
                    Console.Write(D[Tuple.Create(backbones[i], backbones[j])].ToString("f2") + "\t");
                Console.Write("\n");
            }

            Console.ReadKey();
        }
    }
}
