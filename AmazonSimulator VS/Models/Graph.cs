using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace Models
{
    public class Edge
    {
        private Vector3 _a;
        private Vector3 _b;

        public Edge(Vector3 a, Vector3 b)
        {
            this._a = a;
            this._b = b;
        }

        public Vector3 a { get { return _a; } }
        public Vector3 b { get { return _b; } }

        public double length { get { return (a-b).Length(); } }

        public override Boolean Equals(Object o)
        {
            if(o is Edge)
            {
                Edge oe = (Edge) o;
                return (oe.a.Equals(this.a) && oe.b.Equals(this.b)) || (oe.b.Equals(this.a) && oe.a.Equals(this.b));
            }
            return false;
        }

        public Boolean Contains(Vector3 v)
        {
            return a.Equals(v) || b.Equals(v);
        }
    }

    public class Graph
    {
        private List<Vector3> _vertices;
        private List<Edge> _edges;

        public List<Vector3> Nodes { get => _vertices; }
        public List<Edge> Edges { get => _edges; }

        public List<Vector3> vertices { get { return _vertices; } }

        public Graph(List<Vector3> vertices, List<Edge> edges)
        {
            this._vertices = vertices;
            this._edges = edges;
        }

        public Vector3 findNearestVertex(float x, float y, float z)
        {
            return findNearestVertex(new Vector3(x, y, z));
        }

        public Vector3 findNearestVertex(Vector3 v)
        {
            Vector3 nearest = _vertices[0];

            foreach (Vector3 o in _vertices)
            {
                if ((o-v).Length() < (nearest-v).Length()) nearest = o;
            }

            return nearest;
        }

        public List<Vector3> findAdjacentNodes(Vector3 v)
        {
            if (!_vertices.Contains(v))
                throw new ArgumentException("Supplied Vertex is not in this Graph.");

            List<Vector3> adjacents = new List<Vector3>();

            foreach(Edge e in _edges)
            {
                if(e.a.Equals(v)) adjacents.Add(e.b);
                else if(e.b.Equals(v)) adjacents.Add(e.a);
            }

            return adjacents;
        }

        internal void IntegrateVerticesToNearestEdge(List<Vector3> vertices)
        {
            List<Vector3> newPointsOnCurrentGraph = new List<Vector3>();
            List<Edge> newEdgesOnCurrentGraph = new List<Edge>();

            foreach(Vector3 vertex in vertices)
            {
                Vector3 nearestPoint;
                Edge nearestEdge;

                nearestEdge = Edges[0];
                nearestPoint = FindNearestPointToPointOnEdge(nearestEdge, vertex);

                // Find nearest edge and nearest point on this edge
                foreach(Edge e in Edges)
                {
                    Vector3 current = FindNearestPointToPointOnEdge(e, vertex);
                    if ((nearestPoint - vertex).Length() > (current - vertex).Length())
                    {
                        nearestEdge = e;
                        nearestPoint = current;
                    }
                }

                // Create new point on this edge
                // Create new edge consisting of newly found point and vertex
                Edge a = new Edge(vertex, nearestPoint);
                Edge b = new Edge(nearestPoint, nearestEdge.a);
                Edge c = new Edge(nearestPoint, nearestEdge.b);

                // Add vertex, new point to _vertices
                newPointsOnCurrentGraph.Add(nearestPoint);
                newPointsOnCurrentGraph.Add(vertex);

                // Add new Edge to _edges
                newEdgesOnCurrentGraph.Add(a);
                newEdgesOnCurrentGraph.Add(b);
                newEdgesOnCurrentGraph.Add(c);
            }

            this._vertices.AddRange(newPointsOnCurrentGraph);
            this._edges.AddRange(newEdgesOnCurrentGraph);
        }

        private static Vector3 FindNearestPointToPointOnEdge(Edge e, Vector3 p)
        {
            return NearestPointOnFiniteLine(e.a, e.b, p);
        }

        // Source: https://forum.unity.com/threads/how-do-i-find-the-closest-point-on-a-line.340058/
        //linePnt - point the line passes through
        //lineDir - unit vector in direction of line, either direction works
        //pnt - the point to find nearest on line for
        public static Vector3 NearestPointOnFiniteLine(Vector3 start, Vector3 end, Vector3 pnt)
        {
            var line = (end - start);
            var len = line.Length();
            line = Vector3.Normalize(line);

            var v = pnt - start;
            var d = Vector3.Dot(v, line);
            d = Math.Clamp(d, 0f, len);
            return start + line * d;
        }


        /// <summary>
        /// Calculates inter-node distances using Dijkstra's algorithm as specified in https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
        /// </summary>
        /// <param name="source">The source node to calculate distances from.</param>
        /// <param name="destination">The target node to calculate distances to.</param>
        /// <returns>A map containing distances from key node to source node and a previous node if applicable.</returns>
        public static Dictionary<Vector3, Tuple<double, Vector3>> DijkstraDistances(Graph graph, Vector3 source, Vector3 destination)
        {
            Dictionary<Vector3, double> dist = new Dictionary<Vector3, double>();
            Dictionary<Vector3, Vector3> prev = new Dictionary<Vector3, Vector3>();

            List<Vector3> unvisited = new List<Vector3>();

            foreach (Vector3 v in graph.vertices)
            {
                dist[v] = Double.MaxValue;
                unvisited.Add(v);
                prev[v] = v;
            }

            dist[source] = 0.0; // Distance from source is always zero
            unvisited.Remove(source);

            // Find distances to nodes adjacent to source node
            foreach(Vector3 a in graph.findAdjacentNodes(source))
            {
                if ((a-source).Length() < dist[a])
                {
                    dist[a] = (a-source).Length();
                    prev[a] = source;
                }
            }

            while(unvisited.Count > 0)
            {
                unvisited.Sort((a, b) => dist[a].CompareTo(dist[b])); // Sort unvisited ascending by distance from source node

                Vector3 current = unvisited[0];
                unvisited.Remove(current);
                foreach (Vector3 a in graph.findAdjacentNodes(current))
                {
                    if (!unvisited.Contains(a)) continue; // No need to update nodes that have already been visited
                    double nDist = dist[current] + (a-current).Length();
                    if (nDist < dist[a])
                    {
                        dist[a] = nDist; // Update distance to Node A only, and only if it is the smallest distance yet found.
                        prev[a] = current;
                    }
                }
            }

            Dictionary<Vector3, Tuple<double, Vector3>> vals = new Dictionary<Vector3, Tuple<double, Vector3>>();
            foreach(Vector3 key in dist.Keys)
            {
                vals.Add(key, new Tuple<double, Vector3>(dist[key], prev[key]));
            }

            return vals;
        }

        /// <summary>
        /// Finds the shortest path from Node A to B on this graph using Dijkstra's algorithm: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
        /// </summary>
        /// <param name="source">The starting node.</param>
        /// <param name="destination">The target node.</param>
        /// <returns>A list of vertices representing the shortest path from passed Source Node to Target Node.</returns>
        public static List<Vector3> DijkstraShortestPath(Graph graph, Vector3 source, Vector3 destination)
        {
            Dictionary<Vector3, Tuple<double, Vector3>> data = DijkstraDistances(graph, source, destination);

            List<Vector3> path = new List<Vector3>();
            path.Add(destination);

            Vector3 next = destination;
            while(next != source)
            {
                next = data[next].Item2;
                path.Add(next);
            }

            path.Reverse();

            return path;
        }
    }
}