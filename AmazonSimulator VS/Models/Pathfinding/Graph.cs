using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Geometry;

namespace Models
{
    public class Node
    {
        public static implicit operator Vector3(Node n) => n.Position;

        public readonly Vector3 Position;
        public readonly float Width;

        public readonly Graph Parent;

        public Node(Vector3 pos, float w = 0f) { Position = pos; Width = w; }

        public static IEnumerable<Edge> GetDefiningEdges(Node n, Graph g) 
            => g.Edges.Where(edge => edge.Contains(n));

        public static IEnumerable<Edge> GetImplyingEdges(Node n, Graph g)
            => g.Edges.Where(edge => Edge.ImpliesNode(n, edge));

        public static bool IsImpliedOnly(Node n, Graph g)
            => g.Edges.Any(e => !e.Contains(n) && Edge.ImpliesNode(n, e));
        
        public override bool Equals(object obj) => obj is Node ? (obj as Node).Position.Equals(Position) : false;

        public override int GetHashCode()
        {
            var hashCode = 568732793;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(Position);
            return hashCode;
        }
    }

    public class ImpliedNode : Node
    {
        public readonly Edge ImpliedBy;

        public List<Node> Adjacents
            => new List<Node>() { ImpliedBy.A, ImpliedBy.B };

        public ImpliedNode(Vector3 pos, Edge impliedBy) : base(pos, impliedBy.Width)
            => this.ImpliedBy = impliedBy;
    }

    public static class GraphExtensions
    {
        public static (Vector3 Position, float Width) NearestTo(this List<(Vector3 Position, float Width)> nodes, Vector3 p) 
            => nodes.OrderBy(n => Vector3.Distance(n.Position, p)).FirstOrDefault();
    }

    public class Graph
    {
        private List<Node> _nodes;
        private List<Edge> _edges;

        public IEnumerable<Vector3> Vertices { get { return _nodes.Select((n) => n.Position); } }
        public IReadOnlyCollection<Node> Nodes { get => _nodes.AsReadOnly(); }
        public IReadOnlyCollection<Edge> Edges { get => _edges.AsReadOnly(); }

        public Graph(List<Edge> edges)
        {
            this._nodes = new List<Node>();
            this._edges = edges;

            foreach (Edge e in _edges)
            {
                if (!this._nodes.Contains(e.A)) _nodes.Add(e.A);
                if (!this._nodes.Contains(e.B)) _nodes.Add(e.B);
            }
        }

        public bool ImpliesNodeAt(Vector3 pos)
            => DefinesNodeAt(pos)
            || Vector3.Distance(Graph.NearestImpliedNodeTo(pos, this).Position, pos) == 0f;
        
        public bool DefinesNodeAt(Vector3 pos) 
            => Nodes.Any((n) => n.Position.Equals(pos));

        private Node GetNodeAt(Vector3 pos) 
            => Nodes.First(n => n.Position.Equals(pos));

        public static Node NearestNodeTo(Vector3 v, Graph g, bool allowImplied = false)
            => allowImplied && g.DefinesNodeAt(v) 
            ? Graph.NearestExplicitNodeTo(v, g)
            : Graph.NearestImpliedNodeTo(v, g); 
        
        public static Node NearestExplicitNodeTo(Vector3 v, Graph g)
            => g.Nodes.OrderBy(n => Vector3.Distance(n, v)).FirstOrDefault();

        public static ImpliedNode NearestImpliedNodeTo(Node n, Graph g)
            => new ImpliedNode(NearestColinearPointOn(n, g, out Edge edge), edge);
        

        public static Node NearestImpliedNodeTo(Vector3 v, Graph g) => new Node(Graph.NearestColinearPointOn(v, g, out Edge edge), edge.Width);

        public static Vector3 NearestColinearPointOn(Vector3 v, Graph g, out Edge edgeOut)
        {
            var result = g.Edges
                .Select(current => 
                    new {
                        point = LineSegment.NearestColinearPointOn(v, current),
                        edge = current
                    }
                ).OrderBy(vals => Vector3.Distance(vals.point, v))
                .FirstOrDefault();

            edgeOut = result.edge;
            return result.point;
        }

        public static List<Node> NodesAdjacentTo(Node v, Graph g)
        {
            if (v is ImpliedNode) return (v as ImpliedNode).Adjacents;

            if (!g.Nodes.Contains(v))
                throw new ArgumentException("Supplied Vertex is not in this Graph.");

            List<Node> adjacents = new List<Node>();

            foreach(Edge e in g.Edges)
            {
                if(e.A.Position.Equals(v.Position)) adjacents.Add(e.B);
                else if(e.B.Position.Equals(v.Position)) adjacents.Add(e.A);
            }

            return adjacents;
        }

        public float GetEdgeWidth(Edge edge)
        {
            foreach (Edge e in Edges)
                if (e.Equals(edge)) return e.Width;

            return 0f;
        }

        public void IntegrateVerticesToNearestEdge(List<Vector3> vertices, float width = 0f)
        {
            List<Node> newPointsOnCurrentGraph = new List<Node>();
            List<Edge> newEdgesOnCurrentGraph = new List<Edge>();

            foreach(Vector3 vertex in vertices)
            {
                // This vertex already exists on this graph!
                if (this.DefinesNodeAt(vertex)) continue;

                Vector3 nearestPoint;
                Edge nearestEdge;

                nearestEdge = _edges[0];
                nearestPoint = LineSegment.NearestColinearPointOn(vertex, nearestEdge);

                // Find nearest edge and nearest point on this edge
                foreach(Edge e in Edges)
                {
                    Vector3 current = LineSegment.NearestColinearPointOn(vertex, e);
                    if ((nearestPoint - vertex).Length() > (current - vertex).Length())
                    {
                        nearestEdge = e;
                        nearestPoint = current;
                    }
                }

                // Create new point on this edge
                // Create new edge consisting of newly found point and vertex
                Edge a = new Edge(vertex, nearestPoint, width); // New edge should assume the supplied width

                if(!this.DefinesNodeAt(nearestPoint))
                {   // Only insert a new node at the nearest point if there isn't one already.
                    // Connect new vertex to neighboring vertices with new edge, copy original edge's width
                    newEdgesOnCurrentGraph.Add(new Edge(nearestPoint, nearestEdge.A.Position, nearestEdge.Width));
                    newEdgesOnCurrentGraph.Add(new Edge(nearestPoint, nearestEdge.B.Position, nearestEdge.Width));
                    newPointsOnCurrentGraph.Add(a.B);
                }

                // Add new node at current vertex
                newPointsOnCurrentGraph.Add(a.A);

                // Add new Edge connecting current vertex to it's nearest edge on graph
                newEdgesOnCurrentGraph.Add(a);

            }

            this._nodes.AddRange(newPointsOnCurrentGraph);
            this._edges.AddRange(newEdgesOnCurrentGraph);
        }

        /// <summary>
        /// Calculates inter-node distances using Dijkstra's algorithm as specified in https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
        /// </summary>
        /// <param name="src">The source node to calculate distances from.</param>
        /// <param name="destination">The target node to calculate distances to.</param>
        /// <returns>A map containing distances from key node to source node and a previous node if applicable.</returns>
        public static Dictionary<Node, Tuple<double, Node>> DijkstraDistances(Graph graph, Node source)
        {
            if(!(source is ImpliedNode) && !graph.Nodes.Contains(source))
                throw new InvalidOperationException("Node 'src' does not exist on Graph 'graph'.");

            Dictionary<Node, double> dist = new Dictionary<Node, double>();
            Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

            List<Node> unvisited = new List<Node>(); // Used internally only.
            var nodes = graph.Nodes;

            foreach (Node n in nodes)
            {
                dist[n] = Double.MaxValue;
                unvisited.Add(n);
                prev[n] = n;
            }

            // Distance from source is always zero
            dist[source] = 0.0;
            // prev[source] = source implies distance to 'source' is evaluated
            prev[source] = source;

            // Find distances to nodes adjacent to source node
            foreach(Node a in Graph.NodesAdjacentTo(source, graph))
            {
                if (Vector3.Distance(a.Position, source.Position) < dist[a])
                {
                    dist[a] = (a.Position - source.Position).Length();
                    prev[a] = source;
                }
            }

            // All adjacent nodes to source have now been evaluated with respect to source, source is no longer unvisited
            unvisited.Remove(source);

            while(unvisited.Count > 0)
            {
                unvisited.Sort((a, b) => dist[a].CompareTo(dist[b])); // Sort unvisited ascending by distance from source node

                Node current = unvisited[0];
                unvisited.Remove(current);

                foreach (Node a in Graph.NodesAdjacentTo(current, graph))
                {
                    if (!unvisited.Contains(a)) continue; // No need to update nodes that have already been visited
                    double nDist = dist[current] + (a.Position - current.Position).Length();
                    if (nDist < dist[a])
                    {
                        dist[a] = nDist; // Update distance to Node A only, and only if it is the smallest distance yet found.
                        prev[a] = current;
                    }
                }
            }

            Dictionary<Node, Tuple<double, Node>> vals = new Dictionary<Node, Tuple<double, Node>>();

            foreach(Node key in dist.Keys)
                vals.Add(key, new Tuple<double, Node>(dist[key], prev[key]));
            
            return vals;
        }

        /// <summary>
        /// Finds the shortest path from Node A to B on this graph using Dijkstra's algorithm: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
        /// </summary>
        /// <param name="source">The starting node.</param>
        /// <param name="destination">The target node, if this vertex is not mapped on passed graph, a path to destination via nearest node that is present on graph g is returned.</param>
        /// <returns>A list of vertices representing the shortest path from passed Source Node to Target Node.</returns>
        public static List<Node> DijkstraShortestPath(Graph graph, Node source, Node destination)
        {
            ImpliedNode impliedSource = null;

            if (graph.DefinesNodeAt(source)) { }
            else if (graph.ImpliesNodeAt(source)) impliedSource = Graph.NearestImpliedNodeTo(source, graph);
            else throw new InvalidOperationException("Graph does not contain node 'source'!");
            
            Dictionary<Node, Tuple<double, Node>> data = DijkstraDistances(graph, impliedSource != null ? impliedSource : source);

            Node next = null;
            ImpliedNode impliedDestination = null;
            List<Node> path = new List<Node>();

            if (!graph.Nodes.Contains(destination))
            {
                // Fetch the nearest point on any edge of the graph => implied
                impliedDestination = Graph.NearestImpliedNodeTo(destination, graph);
                // Add 'implied' node as final destination on graph
                path.Add(impliedDestination);

                // As DijkstraDistances() doesn't calculate distances to implicit Nodes (Nodes on a line spanned by a graph's explicit nodes) ...
                // ... the final steps to a Node between explicit nodes have to be determined manually
                next = impliedDestination.Adjacents.OrderBy(node => data[node].Item1).First(); // Final explicitly defined node on graph.
            } // The destination node appears to be explicitly defined (Node is defined in Graph.Nodes)
            else next = destination;

            do
            {   // Backtrack the shortest route from source to destination
                path.Add(next);
                next = data[next].Item2;
            }   // Keep backtracking until the given starting position is reached
            while (next.Position != source.Position);

            path.Add(next);

            // Path has been backtracked, reverse it
            return new List<Node>(Enumerable.Reverse(path));
        }
    }
}