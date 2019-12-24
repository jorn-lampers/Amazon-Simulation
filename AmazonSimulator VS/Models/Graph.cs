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
                Edge oe = (Edge)o;
                return (oe.a.Equals(this.a) && oe.b.Equals(this.b)) || (oe.b.Equals(this.a) && oe.a.Equals(this.b));
            }
            return false;
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

        public LinkedList<Vertex> findShortestRoute(Vertex source, Vertex destination)
        {
            if (!_vertices.Contains(source) || !_vertices.Contains(destination))
                throw new ArgumentException("Either one or both supplied vertices are non-existant in Graph.");

            LinkedList<Vertex> route = new LinkedList<Vertex>();
            route.AddFirst(source);

            return route; // TODO: Implement this
        }
    }
}