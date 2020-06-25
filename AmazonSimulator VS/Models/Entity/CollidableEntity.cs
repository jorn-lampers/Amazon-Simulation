using System.Collections.Generic;
using System.Numerics;

namespace Models
{
    public abstract class CollidablePathfindingEntity 
        : PathfindingEntity
        , ICollidable<PolygonalFootprint>
    {
        protected PolygonalFootprint _footprint;

        public CollidablePathfindingEntity(string type, EntityEnvironmentInfoProvider parent, float x, float y, float z, float rotationX, float rotationY, float rotationZ, float movementPerSecond, float rotationPerSecond, float accelerationPerSecond)
            : base(type, parent, x, y, z, rotationX, rotationY, rotationZ, movementPerSecond, rotationPerSecond, accelerationPerSecond)
        {
            this._footprint = new PolygonalFootprint(
                this, 
                new List<Vector2>() {
                    new Vector2(-0.4f, -0.4f),
                    new Vector2(-0.4f, +0.4f),
                    new Vector2(+0.4f, +0.4f),
                    new Vector2(+0.4f, -0.4f)
                }
            );
        }
        public PolygonalFootprint Intersectable 
            => _footprint;

        public bool CheckCollision(PolygonalFootprint other) 
            => this.Intersectable.Intersects(other);

        public override Vector3 Move(Vector3 pos)
            => Move(pos.X, pos.Y, pos.Z);

        public override Vector3 Move(float x, float y, float z)
        {
            Vector3 oldPos = Position;
            base.Move(x, y, z);

            if (_environment.GetCollisions(this).Count > 0)
            { 
                // If the entity were to intersect with some other entity ...
                // undo the movement and set entity speed to 0
                Move(oldPos);
                this._velocity = 0f;
            }

            return Position;
        }
    }
}
