using MentorAlgorithm.Algorithm;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests;

namespace Test
{
    [TestFixture]
    public class MentorTest
    {
        private List<Node> _nodes;
        private Dictionary<Tuple<Node, Node>, int> _graph;
        [SetUp]
        public void Setup()
        {
            _nodes = new List<Node>();
            _nodes.Add(new Node(3, 0, "3"));
            _nodes.Add(new Node(8, 0, "8"));
            _nodes.Add(new Node(13, 0, "13"));
            _nodes.Add(new Node(15, 0, "15"));
            _nodes.Add(new Node(38, 0, "38"));
            _nodes.Add(new Node(50, 0, "50"));

            _graph = new Dictionary<Tuple<Node, Node>, int>();
            _graph.Add(Tuple.Create(_nodes[0], _nodes[1]), 0); //3-8
            _graph.Add(Tuple.Create(_nodes[1], _nodes[0]), 0);
            _graph.Add(Tuple.Create(_nodes[0], _nodes[4]), 0); //3-38
            _graph.Add(Tuple.Create(_nodes[4], _nodes[0]), 0);
            _graph.Add(Tuple.Create(_nodes[4], _nodes[2]), 0); //38-13
            _graph.Add(Tuple.Create(_nodes[2], _nodes[4]), 0);
            _graph.Add(Tuple.Create(_nodes[1], _nodes[5]), 0); //8-50
            _graph.Add(Tuple.Create(_nodes[5], _nodes[1]), 0);
            _graph.Add(Tuple.Create(_nodes[3], _nodes[5]), 0); //15-50
            _graph.Add(Tuple.Create(_nodes[5], _nodes[3]), 0);

        }

        [Test]
        public void DFSGraphTest1()
        {
            List<Node> path = new List<Node>(_nodes.Count);
            Dictionary<Node, bool> visited = new Dictionary<Node, bool>();
            bool flag = false;
            for (int i = 0; i < _nodes.Count; i++)
                visited[_nodes[i]] = false;

            visited[_nodes[2]] = true;
            path.Add(_nodes[2]);
            Mentor.DFSGraph(_nodes[2], _nodes[1], _graph, _nodes, ref flag, ref visited, ref path);

            bool actual = path[0].Name == "13" && path[1].Name == "38" && path[2].Name == "3" && path[3].Name == "8";
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void DFSGraphTest2()
        {
            List<Node> path = new List<Node>(_nodes.Count);
            Dictionary<Node, bool> visited = new Dictionary<Node, bool>();
            bool flag = false;
            for (int i = 0; i < _nodes.Count; i++)
                visited[_nodes[i]] = false;

            visited[_nodes[0]] = true;
            path.Add(_nodes[0]);
            Mentor.DFSGraph(_nodes[0], _nodes[3], _graph, _nodes, ref flag, ref visited, ref path);

            bool actual = path[0].Name == "3" && path[1].Name == "8" && path[2].Name == "50" && path[3].Name == "15";
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void ISPTest()
        {
            var backbones = GetNodes();
            var tree = GetTree(backbones);
            var traffics = GetTraffic(backbones);
            Dictionary<Tuple<Node, Node>, int> edges = new Dictionary<Tuple<Node, Node>, int>(); //chuyển từ tree sang edges
            foreach (var p in tree)
            {
                edges.Add(Tuple.Create(p.Key, p.Value), 0);
                edges.Add(Tuple.Create(p.Value, p.Key), 0);
            }

            Dictionary<Tuple<Node, Node>, double> D = new Dictionary<Tuple<Node, Node>, double>();
            //Ma trận khoảng cách decac
            Dictionary<Tuple<Node, Node>, double> d = new Dictionary<Tuple<Node, Node>, double>();
            //Đường đi từ Node a tới Node b
            Dictionary<Tuple<Node, Node>, List<Node>> nodeOnPath = new Dictionary<Tuple<Node, Node>, List<Node>>();

            for (int i = 0; i < backbones.Count; i++)
            {
                D.Add(Tuple.Create(backbones[i], backbones[i]), 0);
                d.Add(Tuple.Create(backbones[i], backbones[i]), 0);
                for (int j = i + 1; j < backbones.Count; j++)
                {
                    var t1 = Tuple.Create(backbones[i], backbones[j]);
                    var t2 = Tuple.Create(backbones[j], backbones[i]);
                    double dOnTree = 0, dDecac = 0;
                    nodeOnPath.Add(t1, new List<Node>());
                    nodeOnPath.Add(t2, new List<Node>());
                    //nodeOnPath[t1] = nodeOnPath[t2] = DistanceOnTree(centerBB, Backbones[i], Backbones[j], _backboneConnect, ref dOnTree);
                    nodeOnPath[t1] = nodeOnPath[t2] = Mentor.DistanceOnGraph(backbones[i], backbones[j], edges, backbones, ref dOnTree);
                    D.Add(t1, dOnTree);
                    D.Add(t2, dOnTree);
                    dDecac = Mentor.Distance(backbones[i], backbones[j]);
                    d.Add(t1, dDecac);
                    d.Add(t2, dDecac);
                }
            }

            Mentor.IncrementalShortestPath(d, D, backbones, traffics, nodeOnPath, 12, 0.8, tree);
        }

        public static List<Node> GetNodes()
        {
            List<Node> backbones = new List<Node>() {
                new Node(4, 1, "3"), //root
                new Node(9, 1, "4"),
                new Node(2, 3, "6"),
                new Node(6, 5, "8"),
                new Node(0, 6, "10"),
                new Node(8, 7, "13"),
                new Node(1, 8, "14")
            };
            return backbones;
        } 

        public static Dictionary<Node, Node> GetTree(List<Node> backbones)
        {
            Dictionary<Node, Node> tree = new Dictionary<Node, Node>();
            tree.Add(backbones[1], backbones[3]);
            tree.Add(backbones[2], backbones[0]);
            tree.Add(backbones[3], backbones[6]);
            tree.Add(backbones[4], backbones[2]);
            tree.Add(backbones[5], backbones[3]);
            tree.Add(backbones[6], backbones[4]);

            return tree;
        }

        public static Dictionary<Tuple<Node, Node>, double> GetTraffic(List<Node> backbones)
        {
            Dictionary<Tuple<Node, Node>, double> traffics = new Dictionary<Tuple<Node, Node>, double>();
            traffics.Add(Tuple.Create(backbones[0], backbones[1]), 5); //T(3, 4) = 5
            //traffics.Add(Tuple.Create(backbones[1], backbones[0]), 5);
            traffics.Add(Tuple.Create(backbones[2], backbones[1]), 6); //T(6, 4) = 6
            //traffics.Add(Tuple.Create(backbones[1], backbones[2]), 6);
            traffics.Add(Tuple.Create(backbones[0], backbones[3]), 10); //T(3, 8) = 10
            //traffics.Add(Tuple.Create(backbones[3], backbones[0]), 10);
            traffics.Add(Tuple.Create(backbones[4], backbones[1]), 6); //T(10, 4) = 6
            //traffics.Add(Tuple.Create(backbones[1], backbones[4]), 6); 
            return traffics;
        }
    }
}
