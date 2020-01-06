using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace Models {
    public abstract class PathfindingEntity : Entity
    {
        private float _movementSpeed;
        List<Vector3> _route;

        public float movementSpeed { get { return _movementSpeed; } }
        public List<Vector3> route { get { return _route; } }

        public PathfindingEntity(string type, float x, float y, float z, float rotationX, float rotationY, float rotationZ, float movementSpeed = 0.35f) : base(type, x, y, z, rotationX, rotationY, rotationZ)
        {
            this._movementSpeed = movementSpeed;
            this._route = new List<Vector3>();
        }

        public void setPathfindingTarget(Vector3 t, Graph g)
        {
            // Move to nearest vertex on graph first if entity isn't there already
            Vector3 nearest = g.findNearestVertex(position);
            if (!this.position.Equals(nearest)) this._route.Add(nearest); 

            // Find the rest of the waypoints used for traversing Graph 'g'
            this._route.AddRange(Graph.DijkstraShortestPath(g, nearest, t));
        }

        public Vector3 getCurrentWaypoint()
        {
            if (this._route.Count == 0) return this.position;
            else return this._route[0];
        }

        public Vector3 nextWaypoint()
        {
            this._route.RemoveAt(0);
            return getCurrentWaypoint();
        }        

        public Vector3 getDestinationWaypoint()
        {
            if (this._route == null || this._route.Count == 0) return this.position;
            else return this._route[this._route.Count - 1];
        }

        public bool isAtDestination()
        {
            return this.getDestinationWaypoint().Equals(this.position);
        }

        public override bool Update(int tick)
        {
            Vector3 target = this.getCurrentWaypoint();

            if (this.position.Equals(getDestinationWaypoint()))
            {   // Entity has reached its final pathfinding waypoint
                //Console.WriteLine("Robot has reached its final destination.");
                return base.Update(tick);
            }
            else
            {
                float distance = this._movementSpeed;
                if ((target-this.position).Length() < this._movementSpeed)
                {   // If Entity's distance moved this tick exceeds the distance to the next waypoint ...
                    Move((float)target.X, (float)target.Y, (float)target.Z); // ... move to next waypoint ...
                    distance -= (target- this.position).Length(); // ... calculate remaining distance to move ...
                    target = nextWaypoint(); // ... and cycle to the next waypoint.
                }

                if (target.Equals(this.position))
                {   // Entity has reached its destination waypoint
                    //Console.WriteLine("Entity has reached its final destination.");
                    return base.Update(tick);
                }

                // Move entity over the vector spanned between target position and current position with length == distance
                Move(this.position + Vector3.Normalize(target - this.position) * distance);
            }

            // Base class (Entity) determines whether or not a gfx update will be required
            return base.Update(tick);
        }

    }
}