using Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Models
{
    public class Edge
    {
        public static implicit operator LineSegment(Edge v) => new LineSegment(v.A.Position, v.B.Position);
        public static explicit operator Edge(LineSegment l) => new Edge(l);

        private Node _a;
        private Node _b;

        public Edge(Vector3 a, Vector3 b, float width = 0f)
        {
            if (a == b) throw new InvalidOperationException("Edge's limits are equal!");

            this._a = new Node(a, width);
            this._b = new Node(b, width);
        }

        /*
        public Edge(Node a, Node b)
        {
            if (a.Position == b.Position) throw new InvalidOperationException("Edge's limits are equal!");

            this._a = a;
            this._b = b;
        }
        */
        public static bool ImpliesNode(Node n, Edge e, float maxError = 0.001f)
        {
            float dist = Vector3.Distance(LineSegment.NearestColinearPointOn(n, e), n.Position);
            return dist < maxError;
        }

        public Edge(LineSegment l)
        {
            this._a = new Node(l.P);
            this._b = new Node(l.Q);
        }

        public Node A { get { return _a; } }
        public Node B { get { return _b; } }

        public double Length { get => (A.Position - B.Position).Length(); }
        public float Width { get => Math.Min(_a.Width, _b.Width); }
        public Vector3 Direction { get => Vector3.Normalize(B.Position - A.Position); }

        public Node Other(Node n)
        {
            if (this._a.Position == n.Position) return this._b;
            else if (this._b.Position == n.Position) return this._a;
            throw new InvalidOperationException("Edge does not define supplied Node 'n'.");
        }

        public override Boolean Equals(Object o)
        {
            if (o is Edge)
            {
                Edge oe = (Edge)o;
                return (oe.A.Position.Equals(this.A.Position) && oe.B.Position.Equals(this.B.Position)) || (oe.B.Position.Equals(this.A.Position) && oe.A.Position.Equals(this.B.Position));
            }
            return false;
        }

        public Boolean Contains(Vector3 v) => A.Position.Equals(v) || B.Position.Equals(v);

        public Boolean Contains(Node n) => A.Equals(n) || B.Equals(n);

        public override int GetHashCode()
        {
            var hashCode = 722463241;
            hashCode = hashCode * -1521134295 + EqualityComparer<Node>.Default.GetHashCode(_a);
            hashCode = hashCode * -1521134295 + EqualityComparer<Node>.Default.GetHashCode(_b);
            return hashCode;
        }
    }
}
