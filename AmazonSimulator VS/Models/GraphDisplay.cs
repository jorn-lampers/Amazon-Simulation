using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Models
{
    public class GraphDisplay : Entity
    {
        private Graph _graph;

        public List<Vector3> Nodes { get => _graph.Nodes; }
        public List<Edge> Edges { get => _graph.Edges; }

        public GraphDisplay(Graph g) : base("graphdisplay")
        {
            this._graph = g;
        }
    }

}
