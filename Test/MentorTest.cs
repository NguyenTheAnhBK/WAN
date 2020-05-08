using MentorAlgorithm.Algorithm;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
