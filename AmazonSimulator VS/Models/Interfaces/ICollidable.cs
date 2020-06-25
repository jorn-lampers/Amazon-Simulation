using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Models
{
    public interface ICollidable
    {
        IIntersectable Intersectable { get; }
        bool CheckCollision(ICollidable other);
    }

    public interface ICollidable<T> where T : IIntersectable<T>
    {
        T Intersectable { get; }
        bool CheckCollision(T other);
    }

    public interface IIntersectable
    {
        bool Intersects(object other);
    }

    public interface IIntersectable<T>
    {
        bool Intersects(T other);
    }

    public enum Orientation
    {
        CW,     // Clockwise
        CCW,    // Counterclockwise
        CL      // Colinear
    }

    public class LineSegment2 : IIntersectable<LineSegment2>
    {
        public readonly Vector2 P, Q;

        public LineSegment2(Vector2 p1, Vector2 p2)
        {
            this.P = p1;
            this.Q = p2;
        }

        public LineSegment2 at(Vector2 trans)
        {
            return new LineSegment2(P + trans, Q + trans);
        }

        // derived from source: https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
        public bool Contains(Vector2 P)
        {
            if (P.X <= Math.Max(this.P.X, this.Q.X) && P.X >= Math.Min(this.P.X, this.Q.X) &&
            P.Y <= Math.Max(this.P.Y, this.Q.Y) && P.Y >= Math.Min(this.P.Y, this.Q.Y))
                return true;

            return false;
        }

        // derived from source: https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
        public bool Intersects(LineSegment2 other)
        {
            // Find the four orientations needed for general and 
            // special cases 
            Orientation o1 = new Triplet2(this.P, this.Q, other.P).GetOrientation();
            Orientation o2 = new Triplet2(this.P, this.Q, other.Q).GetOrientation();
            Orientation o3 = new Triplet2(other.P, other.Q, this.P).GetOrientation();
            Orientation o4 = new Triplet2(other.P, other.Q, this.Q).GetOrientation();

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == Orientation.CL && this.Contains(other.P)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == Orientation.CL && this.Contains(other.Q)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == Orientation.CL && other.Contains(this.P)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == Orientation.CL && other.Contains(this.Q)) return true;

            return false; // Doesn't fall in any of the above cases 

        }
    }

    public class Triplet2
    {
        public readonly Vector2 P, Q, R;

        public Triplet2(Vector2 p, Vector2 q, Vector2 r)
        {
            this.P = p;
            this.Q = q;
            this.R = r;
        }

        // derived from source: https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
        public Orientation GetOrientation()
        {
            float val = (Q.Y - P.Y) * (R.X - Q.X) -
                    (Q.X - P.X) * (R.Y - Q.Y);

            if (val == 0f) return Orientation.CL;

            return (val > 0f) ? Orientation.CW : Orientation.CCW;
        }
    }

 
}
