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

    public List<LineSegment2> AsLineSegments() => AsLineSegments<List<LineSegment2>>();

    public T AsLineSegments<T>() where T : ICollection<LineSegment2>, new()
    {
        T segments = new T();

        for (var current = _polygon.First; current != _polygon.Last; current = current.Next)
            segments.Add(new LineSegment2(current.Value + new Vector2(_parent.X, _parent.Z), current.Next.Value + new Vector2(_parent.X, _parent.Z)));

        segments.Add(new LineSegment2(_polygon.Last.Value + new Vector2(_parent.X, _parent.Z), _polygon.First.Value + new Vector2(_parent.X, _parent.Z)));

        return segments;
    }

};
}
