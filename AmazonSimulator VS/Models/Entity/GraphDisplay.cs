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

        public IReadOnlyCollection<Node> Nodes { get => _graph.Nodes; }
        public IReadOnlyCollection<Edge> Edges { get => _graph.Edges; }

        public GraphDisplay(EntityEnvironmentInfoProvider parent, Graph g) : base("graphdisplay", parent)
        {
            this._graph = g;
        }
    }

}
