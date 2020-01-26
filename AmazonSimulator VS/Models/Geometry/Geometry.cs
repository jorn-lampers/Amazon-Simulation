using System;
using System.Numerics;

namespace Geometry
{
    public class LineSegment
    {
        public static explicit operator LineSegment(Vector3 v) => new LineSegment(new Vector3(0f,0f,0f), v);
        public static explicit operator Vector3(LineSegment l) => l.Difference;

        public readonly Vector3 P, Q;

        public readonly float Length;

        public readonly Vector3 Difference;
        public readonly Vector3 Direction;

        public LineSegment(Vector3 p, Vector3 q)
        {
            this.P = p;
            this.Q = q;

            this.Difference = Q - P;
            this.Direction = Vector3.Normalize(Difference);
            this.Length = (Q - P).Length();
        }

        public static Vector3 NearestColinearPointOn(Vector3 point, LineSegment l)
        {
            Vector3 toLineBase = point - l.P;
            float lineDot = Vector3.Dot(toLineBase, l.Direction);
            Vector3 intersectAtDiff = l.Direction * Math.Clamp(lineDot, 0f, l.Length);
            return l.P + intersectAtDiff;
        }
    }
}
