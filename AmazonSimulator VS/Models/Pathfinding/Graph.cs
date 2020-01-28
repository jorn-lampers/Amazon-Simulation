using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Geometry;

namespace Models
{
    public class Graph
    {
        private List<Node> _nodes;
        private List<Edge> _edges;

        public IEnumerable<Vector3> Vertices  => _nodes.Select((n) => n.Position); 
        public IReadOnlyCollection<Node> Nodes => _nodes.AsReadOnly(); 
        public IReadOnlyCollection<Edge> Edges => _edges.AsReadOnly(); 

        public Graph(Edge[] edges)
        {
            this._nodes = new List<Node>();
            this._edges = new List<Edge>(edges);

            foreach (Edge e in _edges)
            {
                if (!this._nodes.Contains(e.A)) _nodes.Add(e.A);
                if (!this._nodes.Contains(e.B)) _nodes.Add(e.B);
            }
        }

        public bool ImpliesNodeAt(Vector3 pos)
        {
            return DefinesNodeAt(pos) || Vector3.Distance(NearestImpliedNodeTo(pos).Position, pos) == 0f;
        }

        public bool DefinesNodeAt(Vector3 pos)
        {
            return Nodes.Any((n) => n.Position.Equals(pos));
        }

        public Node GetNodeAt(Vector3 pos)
        {
            return Nodes.First(n => n.Position.Equals(pos));
        }

        public Node NearestNodeTo(Vector3 v, bool allowImplied = false)
        {
            return allowImplied && DefinesNodeAt(v)
            ? NearestExplicitNodeTo(v)
            : NearestImpliedNodeTo(v);
        }

        public Node NearestExplicitNodeTo(Vector3 v)
        {
            return Nodes.OrderBy(n => Vector3.Distance(n, v)).FirstOrDefault();
        }

        public ImpliedNode NearestImpliedNodeTo(Node n)
        {
            return new ImpliedNode(NearestColinearPointOn(n, out Edge edge), edge);
        }

        public Node NearestImpliedNodeTo(Vector3 v)
        {
            return new Node(NearestColinearPointOn(v, out Edge edge), edge.Width);
        }

        private Vector3 NearestColinearPointOn(Vector3 v, out Edge edgeOut)
        {
            var result = Edges
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

        public List<Node> NodesAdjacentTo(Node v)
        {
            if (v is ImpliedNode) return (v as ImpliedNode).Adjacents;

            if (!Nodes.Contains(v))
                throw new ArgumentException("Supplied Vertex is not in this Graph.");

            List<Node> adjacents = new List<Node>();

            foreach(Edge e in Edges)
            {
                if(e.A.Position.Equals(v.Position)) adjacents.Add(e.B);
                else if(e.B.Position.Equals(v.Position)) adjacents.Add(e.A);
            }

            return adjacents;
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
        private Dictionary<Node, Tuple<double, Node>> DijkstraDistances(Node source)
        {
            if(!(source is ImpliedNode) && !Nodes.Contains(source))
                throw new InvalidOperationException("Node 'src' does not exist on Graph 'graph'.");

            Dictionary<Node, double> dist = new Dictionary<Node, double>();
            Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

            List<Node> unvisited = new List<Node>(); // Used internally only.

            foreach (Node n in Nodes)
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
            foreach(Node a in NodesAdjacentTo(source))
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

                foreach (Node a in NodesAdjacentTo(current))
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
        public List<Node> DijkstraShortestPath(Node source, Node destination)
        {   // TODO: Graph.DijkstraShortestPath() shouldn't return any duplicates...
            ImpliedNode impliedSource = null;

            if (DefinesNodeAt(source)) { }
            else if (ImpliesNodeAt(source)) impliedSource = NearestImpliedNodeTo(source);
            else throw new InvalidOperationException("Graph does not contain node 'source'!");
            
            Dictionary<Node, Tuple<double, Node>> data = DijkstraDistances(impliedSource != null ? impliedSource : source);

            Node next = null;
            ImpliedNode impliedDestination = null;
            List<Node> path = new List<Node>();

            if (!Nodes.Contains(destination))
            {
                // Fetch the nearest point on any edge of the graph => implied
                impliedDestination = NearestImpliedNodeTo(destination);
                // Add 'implied' node as final destination on graph
                path.Add(impliedDestination);

                // As DijkstraDistances() doesn't calculate distances to implicit Nodes (Nodes on a line spanned by a graph's explicit nodes) ...
                // ... the final steps to a Node between explicit nodes have to be determined manually
                next = impliedDestination.Adjacents.OrderBy(node => data[node].Item1).First(); // Final explicitly defined node on graph.
            } // The destination node appears to be explicitly defined (Node is defined in Graph.Nodes)
            else next = destination;

            do
            {   // Backtrack the shortest route from source to destination
                if(!path.Contains(next)) path.Add(next);
                next = data[next].Item2;
            }   // Keep backtracking until the given starting position is reached
            while (next.Position != source.Position);

            path.Add(next);

            // Path has been backtracked, reverse it
            return new List<Node>(Enumerable.Reverse(path));
        }
    }
}