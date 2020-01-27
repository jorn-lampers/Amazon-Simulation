using System.Collections.Generic;

namespace Models
{
    public class GraphDisplay 
        : Entity
    {
        private Graph _graph;

        public IReadOnlyCollection<Node> Nodes => _graph.Nodes; 
        public IReadOnlyCollection<Edge> Edges => _graph.Edges; 

        public GraphDisplay(EntityEnvironmentInfoProvider parent, Graph g) 
            : base("graphdisplay", parent)
            => this._graph = g;
    }
}
