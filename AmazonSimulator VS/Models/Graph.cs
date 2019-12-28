using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Models
{
    public class Vertex
    {
        private double _x = 0;
        private double _y = 0;
        private double _z = 0;

        public double x { get { return _x; } }
        public double y { get { return _y; } }
        public double z { get { return _z; } }
        public Vertex(double x, double y, double z)
        {
            this._x = x;
            this._y = y;
            this._z = z;
        }

        public double distanceTo(Vertex v)
        {
            return Math.Sqrt(
                Math.Pow(this.x - v.x, 2) +
                Math.Pow(this.y - v.y, 2) +
                Math.Pow(this.z - v.z, 2)
            );
        }
    }

    public class Edge
    {
        private Vertex _a;
        private Vertex _b;

        public Edge(Vertex a, Vertex b)
        {
            this._a = a;
            this._b = b;
        }

        public Vertex a { get { return _a; } }
        public Vertex b { get { return _b; } }

        public double length { get { return a.distanceTo(b); } }

        public override Boolean Equals(Object o)
        {
            if(o is Edge)
            {
                Edge oe = (Edge) o;
                return (oe.a.Equals(this.a) && oe.b.Equals(this.b)) || (oe.b.Equals(this.a) && oe.a.Equals(this.b));
            }
            return false;
        }

        public Boolean Contains(Vertex v)
        {
            return a.Equals(v) || b.Equals(v);
        }
    }

    public class Graph
    {
        private List<Vertex> _vertices;
        private List<Edge> _edges;

        public Graph(List<Vertex> vertices, List<Edge> edges)
        {
            this._vertices = vertices;
            this._edges = edges;
        }

        public Vertex findNearestVertex(double x, double y, double z)
        {
            return findNearestVertex(new Vertex(x, y, z));
        }

        public Vertex findNearestVertex(Vertex v)
        {
            if (_vertices == null || _vertices.Count == 0) return null;
            Vertex nearest = _vertices[0];

            foreach (Vertex o in _vertices)
            {
                if (o.distanceTo(v) < nearest.distanceTo(v)) nearest = o;
            }

            return nearest;
        }

        public List<Vertex> findAdjacentNodes(Vertex v)
        {
            if (!_vertices.Contains(v))
                throw new ArgumentException("Supplied Vertex is not in this Graph.");

            List<Vertex> adjacents = new List<Vertex>();

            foreach(Edge e in _edges)
            {
                if(e.a.Equals(v)) adjacents.Add(e.b);
                else if(e.b.Equals(v)) adjacents.Add(e.a);
            }

            return adjacents;
        }

        /// <summary>
        /// Calculates inter-node distances using Dijkstra's algorithm as specified in https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
        /// </summary>
        /// <param name="source">The source node to calculate distances from.</param>
        /// <param name="destination">The target node to calculate distances to.</param>
        /// <returns>A map containing distances from key node to source node and a previous node if applicable.</returns>
        public Dictionary<Vertex, Tuple<double, Vertex>> dijkstraDists(Vertex source, Vertex destination)
        {
            Dictionary<Vertex, double> dist = new Dictionary<Vertex, double>();
            Dictionary<Vertex, Vertex> prev = new Dictionary<Vertex, Vertex>();

            List<Vertex> unvisited = new List<Vertex>();

            foreach (Vertex v in _vertices)
            {
                dist[v] = Double.MaxValue;
                unvisited.Add(v);
                prev[v] = null;
            }

            dist[source] = 0.0; // Distance from source is always zero
            unvisited.Remove(source);

            // Find distances to nodes adjacent to source node
            foreach(Vertex a in findAdjacentNodes(source))
            {
                if (a.distanceTo(source) < dist[a])
                {
                    dist[a] = a.distanceTo(source);
                    prev[a] = source;
                }
            }

            while(unvisited.Count > 0)
            {
                unvisited.Sort((a, b) => dist[a].CompareTo(dist[b])); // Sort unvisited ascending by distance from source node

                Vertex current = unvisited[0];
                unvisited.Remove(current);
                foreach (Vertex a in findAdjacentNodes(current))
                {
                    if (!unvisited.Contains(a)) continue; // No need to update nodes that have already been visited
                    double nDist = dist[current] + a.distanceTo(current);
                    if (nDist < dist[a])
                    {
                        dist[a] = nDist; // Update distance to Node A only, and only if it is the smallest distance yet found.
                        prev[a] = current;
                    }
                }
            }

            Dictionary<Vertex, Tuple<double, Vertex>> vals = new Dictionary<Vertex, Tuple<double, Vertex>>();
            foreach(Vertex key in dist.Keys)
            {
                vals.Add(key, new Tuple<double, Vertex>(dist[key], prev[key]));
            }

            return vals;
        }

        /// <summary>
        /// Finds the shortest path from Node A to B on this graph using Dijkstra's algorithm: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
        /// </summary>
        /// <param name="source">The starting node.</param>
        /// <param name="destination">The target node.</param>
        /// <returns>A list of vertices representing the shortest path from passed Source Node to Target Node.</returns>
        public List<Vertex> dijkstraShortestPath(Vertex source, Vertex destination)
        {
            Dictionary<Vertex, Tuple<double, Vertex>> data = dijkstraDists(source, destination);

            List<Vertex> path = new List<Vertex>();
            path.Add(destination);

            Vertex next = destination;
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