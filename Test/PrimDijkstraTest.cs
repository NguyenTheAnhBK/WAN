using MentorAlgorithm.Algorithm;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class PrimDijkstraTest
    {
        private PrimDijkstra _primDijkstra;
        public Dijkstra _dijkstra;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var nodes = GetNodes();
            var edges = GetEdges(nodes);
            _primDijkstra = new PrimDijkstra(edges, nodes, nodes.Count, 0.4);
            var path = _primDijkstra.FindPath(nodes[0]).ToDictionary(x => x.Key.Name, x => x.Value.Name);
            Dictionary<string, string> expected = new Dictionary<string, string>();
            expected.Add("1", "0");
            expected.Add("2", "0");
            expected.Add("4", "1");
            expected.Add("5", "0");
            expected.Add("6", "1");
            Assert.AreEqual(expected, path);
        }

        [Test]
        public void Test2()
        {
            var nodes = GetNodes();
            var edges = GetEdges(nodes);
            _dijkstra = new Dijkstra(edges, nodes, nodes.Count);
            var path = _dijkstra.FindPath(nodes[0]).ToDictionary(x => x.Key.Name, x => x.Value.Name);
            Dictionary<string, string> expected = new Dictionary<string, string>();
            expected.Add("1", "0");
            expected.Add("2", "0");
            expected.Add("4", "0");
            expected.Add("5", "0");
            expected.Add("6", "1");
            Assert.AreEqual(expected, path);
        }

        List<Node> GetNodes()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(0, 0, "0"));
            nodes.Add(new Node(0, 0, "1"));
            nodes.Add(new Node(0, 0, "2"));
            nodes.Add(new Node(0, 0, "3"));
            nodes.Add(new Node(0, 0, "4"));
            nodes.Add(new Node(0, 0, "5"));
            nodes.Add(new Node(0, 0, "6"));
            return nodes;
        }

        Dictionary<Tuple<Node, Node>, double> GetEdges(List<Node> nodes)
        {
            Dictionary<Tuple<Node, Node>, double> edges = new Dictionary<Tuple<Node, Node>, double>();
            edges.Add(Tuple.Create(nodes[0], nodes[1]), 5);
            edges.Add(Tuple.Create(nodes[0], nodes[2]), 6);
            edges.Add(Tuple.Create(nodes[0], nodes[4]), 7);
            edges.Add(Tuple.Create(nodes[0], nodes[5]), 8);
            edges.Add(Tuple.Create(nodes[0], nodes[6]), 10);
            edges.Add(Tuple.Create(nodes[1], nodes[2]), 9);
            edges.Add(Tuple.Create(nodes[1], nodes[4]), 4);
            edges.Add(Tuple.Create(nodes[1], nodes[5]), 7);
            edges.Add(Tuple.Create(nodes[1], nodes[6]), 3);
            edges.Add(Tuple.Create(nodes[1], nodes[0]), 5);
            edges.Add(Tuple.Create(nodes[2], nodes[0]), 6);
            edges.Add(Tuple.Create(nodes[2], nodes[1]), 8);
            edges.Add(Tuple.Create(nodes[2], nodes[4]), 12);
            edges.Add(Tuple.Create(nodes[2], nodes[5]), 6);
            edges.Add(Tuple.Create(nodes[2], nodes[6]), 8);
            edges.Add(Tuple.Create(nodes[4], nodes[0]), 7);
            edges.Add(Tuple.Create(nodes[4], nodes[1]), 4);
            edges.Add(Tuple.Create(nodes[4], nodes[2]), 12);
            edges.Add(Tuple.Create(nodes[4], nodes[5]), 7);
            edges.Add(Tuple.Create(nodes[4], nodes[6]), 4);
            edges.Add(Tuple.Create(nodes[5], nodes[0]), 8);
            edges.Add(Tuple.Create(nodes[5], nodes[1]), 7);
            edges.Add(Tuple.Create(nodes[5], nodes[2]), 6);
            edges.Add(Tuple.Create(nodes[5], nodes[4]), 7);
            edges.Add(Tuple.Create(nodes[5], nodes[6]), 6);
            edges.Add(Tuple.Create(nodes[6], nodes[0]), 10);
            edges.Add(Tuple.Create(nodes[6], nodes[1]), 3);
            edges.Add(Tuple.Create(nodes[6], nodes[2]), 8);
            edges.Add(Tuple.Create(nodes[6], nodes[4]), 4);
            edges.Add(Tuple.Create(nodes[6], nodes[5]), 6);
            return edges;
        }
    }
}