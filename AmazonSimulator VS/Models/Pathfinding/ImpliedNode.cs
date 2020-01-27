using System.Collections.Generic;
using System.Numerics;

namespace Models
{
    public class ImpliedNode : Node
    {
        public readonly Edge ImpliedBy;

        public List<Node> Adjacents
            => new List<Node>() { ImpliedBy.A, ImpliedBy.B };

        public ImpliedNode(Vector3 pos, Edge impliedBy) 
            : base(pos, impliedBy.Width)
            => this.ImpliedBy = impliedBy;
    }
}
