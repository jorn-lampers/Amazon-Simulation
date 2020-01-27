using Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Models
{
    public class Edge
    {
        /// <summary> 
        /// Allows for implicit type conversion Edge => LineSegment
        /// </summary>
        public static implicit operator LineSegment(Edge v) 
            => new LineSegment(v.A.Position, v.B.Position);

        /// <summary> 
        /// Allows for explicit type conversion LineSegment => Edge (IE. Typecasting)
        /// </summary>
        public static explicit operator Edge(LineSegment l) 
            => new Edge(l);

        private Node _a;
        private Node _b;

        public Edge(Vector3 a, Vector3 b, float width = 0f)
        {
            if (a == b) throw new InvalidOperationException("Edge's limits are equal!");

            this._a = new Node(a, width);
            this._b = new Node(b, width);
        }

        public Edge(LineSegment l)
        {
            this._a = new Node(l.P);
            this._b = new Node(l.Q);
        }

        public Node A 
            => _a;

        public Node B
            => _b;

        public double Length 
            => (A.Position - B.Position).Length(); 

        public float Width 
            => Math.Min(_a.Width, _b.Width); 

        public Vector3 Direction 
            => Vector3.Normalize(B.Position - A.Position); 

        public static bool ImpliesNode(Node n, Edge e)
            => Vector3.Distance(LineSegment.NearestColinearPointOn(n, e), n.Position) == 0f;
        
        public Boolean Contains(Vector3 v) => 
            A.Position.Equals(v) || B.Position.Equals(v);

        public Boolean Contains(Node n) => 
            A.Equals(n) || B.Equals(n);

        public override Boolean Equals(Object o)
            => (o is Edge) 
            ? this.Equals(o as Edge)
            : false;

        public Boolean Equals(Edge o)
            => (o.A.Equals(A) && o.B.Equals(B))
            || (o.B.Equals(A) && o.A.Equals(B));

        public override int GetHashCode()
        {
            var hashCode = 722463241;
            hashCode = hashCode * -1521134295 + EqualityComparer<Node>.Default.GetHashCode(_a);
            hashCode = hashCode * -1521134295 + EqualityComparer<Node>.Default.GetHashCode(_b);
            return hashCode;
        }
    }
}
