using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentorAlgorithm.Algorithm
{
    public class Node: OxyPlot.Series.ScatterPoint
    {
        public Node(double x, double y, string name) : base(x, y)
        {
            this.Name = name;
        }

        
        public string Name { get; set; }
        public int Traffic { get; set; } = 0;
        public int Weight { get; set; } = 0;
        public bool Status { get; set; } = false;
    }
}
