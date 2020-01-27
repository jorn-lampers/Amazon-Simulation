using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Models
{
    public class Node
    {
        public static implicit operator Vector3(Node n) => n.Position;

        public readonly Vector3 Position;
        public readonly float Width;
        public readonly Graph Parent;

        public Node(Vector3 pos, float w = 0f)
        {
            Position = pos;
            Width = w;
        }

        public static IEnumerable<Edge> GetDefiningEdges(Node n, Graph g)
            => g.Edges.Where(edge => edge.Contains(n));

        public static IEnumerable<Edge> GetImplyingEdges(Node n, Graph g)
            => g.Edges.Where(edge => Edge.ImpliesNode(n, edge));

        public static bool IsImpliedOnly(Node n, Graph g)
            => g.Edges.Any(e => !e.Contains(n) && Edge.ImpliesNode(n, e));

        public override bool Equals(object obj)
            => obj is Node 
            ? (obj as Node).Position.Equals(Position) 
            : false;

        public override int GetHashCode()
        {
            var hashCode = 568732793;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(Position);
            return hashCode;
        }
    }
}
