using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Models
{
   /// <summary>
    /// Represents a polygonal 2D-Hitbox
    /// </summary>
    public class PolygonalFootprint : IIntersectable<PolygonalFootprint>
    {
        private LinkedList<Vector2> _polygon;
        private Entity _parent;

        public LinkedList<Vector2> Vertices { get => _polygon; }
        public Vector2 Position { get => new Vector2(_parent.Position.X, _parent.Position.Z); }

        public PolygonalFootprint(Entity parent, IEnumerable<Vector2> e)
        {
            _parent = parent;
            _polygon = new LinkedList<Vector2>(e);
        }
        public bool Intersects(PolygonalFootprint other)
        {
            var tSegs = this.AsLineSegments();
            var oSegs = other.AsLineSegments();

            bool res = tSegs.Any((ts) => oSegs.Any((os) => os.Intersects(ts)));

            return res;
        }

        public bool Intersects(LineSegment2 other)
        {
            var tSegs = this.AsLineSegments();

            bool res = tSegs.Any((ts) => other.Intersects(ts));

            return res;
        }

        public bool Intersects(ICollection<LineSegment2> other)
        {
            var tSegs = this.AsLineSegments();

            bool res = tSegs.Any((ts) => other.Any((os) => os.Intersects(ts)));

            return res;
        }

        public List<LineSegment2> AsLineSegments() => AsLineSegments<List<LineSegment2>>();

        public T AsLineSegments<T>() where T : ICollection<LineSegment2>, new()
        {
            T segments = new T();

            for (var current = _polygon.First; current != _polygon.Last; current = current.Next)
                segments.Add(new LineSegment2(current.Value + new Vector2(_parent.X, _parent.Z), current.Next.Value + new Vector2(_parent.X, _parent.Z)));

            segments.Add(new LineSegment2(_polygon.Last.Value + new Vector2(_parent.X, _parent.Z), _polygon.First.Value + new Vector2(_parent.X, _parent.Z)));

            return segments;
        }

        public T AsTranslatedLineSegments<T>(Vector2 translation) where T : ICollection<LineSegment2>, new()
        {
            T segments = new T();

            for (var current = _polygon.First; current != _polygon.Last; current = current.Next)
                segments.Add(new LineSegment2(current.Value + new Vector2(_parent.X, _parent.Z) + translation, current.Next.Value + new Vector2(_parent.X, _parent.Z) + translation));

            segments.Add(new LineSegment2(_polygon.Last.Value + new Vector2(_parent.X, _parent.Z) + translation, _polygon.First.Value + new Vector2(_parent.X, _parent.Z) + translation));

            return segments;
        }

        public T AsCoveredAreaRect<T>(Vector2 translation) where T : ICollection<LineSegment2>, new()
        {
            T segments = new T();

            var segs = this.AsLineSegments();
            segs.AddRange(this.AsTranslatedLineSegments<List<LineSegment2>>(translation));

            float minX = segs.Select((s) => s.P.X).Min(), minY = segs.Select((s) => s.P.Y).Min();
            float maxX = segs.Select((s) => s.P.X).Max(), maxY = segs.Select((s) => s.P.Y).Max();

            Vector2 A = new Vector2(minX, minY);
            Vector2 B = new Vector2(minX, maxY);
            Vector2 C = new Vector2(maxX, minY);
            Vector2 D = new Vector2(maxX, maxY);

            segments.Add(new LineSegment2(A, B));
            segments.Add(new LineSegment2(A, C));
            segments.Add(new LineSegment2(C, D));
            segments.Add(new LineSegment2(D, B));

            return segments;
        }


    };
}
